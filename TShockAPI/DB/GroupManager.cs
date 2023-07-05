/*
TShock, a server mod for Terraria
Copyright (C) 2011-2019 Pryaxis & TShock Contributors

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using MongoDB.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace TShockAPI.DB
{
	/// <summary>
	/// Represents the GroupManager, which is in charge of group management.
	/// </summary>
	public class GroupManager : IEnumerable<Group>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GroupManager"/> class with the specified database connection.
		/// </summary>
		/// <param name="db">The connection.</param>
		public GroupManager()
		{

			// Add default groups if they don't exist
			AddDefaultGroup("guest", "", Data.DefaultPermissions.Guest);
			AddDefaultGroup("default", "guest", Data.DefaultPermissions.Default);
			AddDefaultGroup("vip", "default", Data.DefaultPermissions.VIP);
			AddDefaultGroup("mod", "vip", Data.DefaultPermissions.Mod);
			AddDefaultGroup("admin", "mod", Data.DefaultPermissions.Admin);
			AddDefaultGroup("owner", "admin", Data.DefaultPermissions.Owner);

			Group.DefaultGroup = GetGroupByName(TShock.Config.Settings.DefaultGuestGroupName);

			AssertCoreGroupsPresent();
		}

		internal void AssertCoreGroupsPresent()
		{
			if (!GroupExists(TShock.Config.Settings.DefaultGuestGroupName))
			{
				TShock.Log.ConsoleError(GetString("The guest group could not be found. This may indicate a typo in the configuration file, or that the group was renamed or deleted."));
				throw new Exception(GetString("The guest group could not be found."));
			}

			if (!GroupExists(TShock.Config.Settings.DefaultRegistrationGroupName))
			{
				TShock.Log.ConsoleError(GetString("The default usergroup could not be found. This may indicate a typo in the configuration file, or that the group was renamed or deleted."));
				throw new Exception(GetString("The default usergroup could not be found."));
			}
		}

		/// <summary>
		/// Asserts that the group reference can be safely assigned to the player object.
		/// <para>If this assertion fails, and <paramref name="kick"/> is true, the player is disconnected. If <paramref name="kick"/> is false, the player will receive an error message.</para>
		/// </summary>
		/// <param name="player">The player in question</param>
		/// <param name="group">The group we want to assign them</param>
		/// <param name="kick">Whether or not failing this check disconnects the player.</param>
		/// <returns></returns>
		public bool AssertGroupValid(TSPlayer player, Group group, bool kick)
		{
			if (group == null)
			{
				if (kick)
					player.Disconnect(GetString("Your account's group could not be loaded. Please contact server administrators about this."));
				else
					player.SendErrorMessage(GetString("Your account's group could not be loaded. Please contact server administrators about this."));
				return false;
			}

			return true;
		}

		private void AddDefaultGroup(string name, string parent, string[] permissions)
		{
			if (!GroupExists(name))
				AddGroup(name, parent, permissions, Group.defaultChatColor);
		}

		/// <summary>
		/// Determines whether the given group exists.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <returns><c>true</c> if it does; otherwise, <c>false</c>.</returns>
		public bool GroupExists(string group)
		{
			if (group == "superadmin")
				return true;
			
			return MongoDB.Entities.DB.Find<Group>().ManyAsync(a => a.Name == group).Result.Any();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator<Group> GetEnumerator() => MongoDB.Entities.DB.Find<Group>().ManyAsync(x => true).Result.GetEnumerator();

		/// <summary>
		/// Gets the group matching the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The group.</returns>
		public Group GetGroupByName(string name) => MongoDB.Entities.DB.Find<Group>().ManyAsync(a => a.Name == name).Result.First();

		public IEnumerable<Group> RetrieveAll() => MongoDB.Entities.DB.Find<Group>().ManyAsync(a => true).Result;

		/// <summary>
		/// Adds group with name and permissions if it does not exist.
		/// </summary>
		/// <param name="name">name of group</param>
		/// <param name="parentname">parent of group</param>
		/// <param name="permissions">permissions</param>
		/// <param name="chatcolor">chatcolor</param>
		public void AddGroup(String name, string parentname, string[] permissions, String chatcolor)
		{
			if (GroupExists(name))
				throw new GroupExistsException(name);

			Group group = new Group(name, null, chatcolor);
			group.Permissions = permissions.ToList();

			group.Parent = ValidParent(parentname, group);
			group.SaveAsync();
		}

		Group ValidParent(string parentname, Group group)
		{
			Group parent = new Group();
			if (!string.IsNullOrWhiteSpace(parentname))
			{
				parent = GetGroupByName(parentname);
				if (parent == null || parent == group)
					throw new GroupManagerException(GetString($"Invalid parent group {parentname} for group {group.Name}."));

				// Check if the new parent would cause loops.
				List<Group> groupChain = new List<Group> { group, parent };
				Group checkingGroup = parent.Parent;
				while (checkingGroup != null)
				{
					if (groupChain.Contains(checkingGroup))
						throw new GroupManagerException(
							GetString($"Parenting group {group} to {parentname} would cause loops in the parent chain."));

					groupChain.Add(checkingGroup);
					checkingGroup = checkingGroup.Parent;
				}
			}
			return parent;
		}
		
		/// <summary>
		/// Updates a group including permissions
		/// </summary>
		/// <param name="name">name of the group to update</param>
		/// <param name="parentname">parent of group</param>
		/// <param name="permissions">permissions</param>
		/// <param name="chatcolor">chatcolor</param>
		/// <param name="suffix">suffix</param>
		/// <param name="prefix">prefix</param> //why is suffix before prefix?!
		public void UpdateGroup(string name, string parentname, string[] permissions, string chatcolor, string suffix, string prefix)
		{
			Group group = GetGroupByName(name);
			if (group is null)
				throw new GroupNotExistException(name);

			Group parent = ValidParent(parentname, group);

			group.Prefix = prefix;
			group.Suffix = suffix;
			group.ChatColor = chatcolor;
			group.Permissions = permissions.ToList();
			group.Parent = parent;
			group.SaveAsync();
		}

		/// <summary>
		/// Renames the specified group.
		/// </summary>
		/// <param name="name">The group's name.</param>
		/// <param name="newName">The new name.</param>
		/// <returns>The result from the operation to be sent back to the user.</returns>
		public string RenameGroup(string name, string newName)
		{
			if (!GroupExists(name))
				throw new GroupNotExistException(name);
			

			if (GroupExists(newName))
				throw new GroupExistsException(newName);

			Group group = GetGroupByName(name);
			if (group is null)
				throw new GroupNotExistException(name);

			if (TShock.Config.Settings.DefaultGuestGroupName == name)
				TShock.Config.Settings.DefaultGuestGroupName = newName;
			if(TShock.Config.Settings.DefaultRegistrationGroupName == name)
				TShock.Config.Settings.DefaultRegistrationGroupName = newName;

			group.Name = newName;
			group.SaveAsync();
			return "Group renamed.";
		}

		/// <summary>
		/// Deletes the specified group.
		/// </summary>
		/// <param name="name">The group's name.</param>
		/// <param name="exceptions">Whether exceptions will be thrown in case something goes wrong.</param>
		/// <returns>The result from the operation to be sent back to the user.</returns>
		public string DeleteGroup(String name, bool exceptions = false)
		{
			if (!GroupExists(name))
			{
				if (exceptions)
					throw new GroupNotExistException(name);
				return GetString($"Group {name} doesn't exist.");
			}

			if (name == Group.DefaultGroup.Name)
			{
				if (exceptions)
					throw new GroupManagerException(GetString("You can't remove the default guest group."));
				return GetString("You can't remove the default guest group.");
			}

			Group deleted = GetGroupByName(name);
			deleted.DeleteAsync();

			if (exceptions)
				throw new GroupManagerException(GetString($"Failed to delete group {name}."));
			return GetString($"Failed to delete group {name}.");
		}

		/// <summary>
		/// Enumerates the given permission list and adds permissions for the specified group accordingly.
		/// </summary>
		/// <param name="name">The group name.</param>
		/// <param name="permissions">The permission list.</param>
		/// <returns>The result from the operation to be sent back to the user.</returns>
		public String AddPermissions(String name, List<String> permissions)
		{
			if (!GroupExists(name))
				return GetString($"Group {name} doesn't exist.");

			var group = TShock.Groups.GetGroupByName(name);
			var oldperms = group.Permissions; // Store old permissions in case of error
			permissions.ForEach(p => group.AddPermission(p));

			group.SaveAsync();
			return "";
		}

		/// <summary>
		/// Enumerates the given permission list and removes valid permissions for the specified group accordingly.
		/// </summary>
		/// <param name="name">The group name.</param>
		/// <param name="permissions">The permission list.</param>
		/// <returns>The result from the operation to be sent back to the user.</returns>
		public String DeletePermissions(String name, List<String> permissions)
		{
			if (!GroupExists(name))
				return GetString($"Group {name} doesn't exist.");

			var group = TShock.Groups.GetGroupByName(name);
			var oldperms = group.Permissions; // Store old permissions in case of error
			permissions.ForEach(p => group.RemovePermission(p));

			group.SaveAsync();
			return "";
		}
			
	}

	/// <summary>
	/// Represents the base GroupManager exception.
	/// </summary>
	[Serializable]
	public class GroupManagerException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GroupManagerException"/> with the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		public GroupManagerException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GroupManagerException"/> with the specified message and inner exception.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner exception.</param>
		public GroupManagerException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// Represents the GroupExists exception.
	/// This exception is thrown whenever an attempt to add an existing group into the database is made.
	/// </summary>
	[Serializable]
	public class GroupExistsException : GroupManagerException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GroupExistsException"/> with the specified group name.
		/// </summary>
		/// <param name="name">The group name.</param>
		public GroupExistsException(string name)
			: base(GetString($"Group {name} already exists"))
		{
		}
	}

	/// <summary>
	/// Represents the GroupNotExist exception.
	/// This exception is thrown whenever we try to access a group that does not exist.
	/// </summary>
	[Serializable]
	public class GroupNotExistException : GroupManagerException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GroupNotExistException"/> with the specified group name.
		/// </summary>
		/// <param name="name">The group name.</param>
		public GroupNotExistException(string name)
			: base(GetString($"Group {name} does not exist"))
		{
		}
	}
}
