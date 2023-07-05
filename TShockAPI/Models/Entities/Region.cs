using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Entity = MongoDB.Entities.Entity;

namespace TShockAPI
{
	public class Region : Entity
	{
		public int ID { get; set; }
		public Rectangle Area { get; set; }
		public string Name { get; set; }
		public string Owner { get; set; }
		public bool DisableBuild { get; set; }
		public string WorldID { get; set; }
		public List<int> AllowedIDs { get; set; }
		public List<string> AllowedGroups { get; set; }
		public int Z { get; set; }

		public Region(int id, Rectangle region, string name, string owner, bool disablebuild, string RegionWorldIDz, int z)
			: this()
		{
			ID = id;
			Area = region;
			Name = name;
			Owner = owner;
			DisableBuild = disablebuild;
			WorldID = RegionWorldIDz;
			Z = z;
		}

		public Region()
		{
			Area = Rectangle.Empty;
			Name = string.Empty;
			DisableBuild = true;
			WorldID = string.Empty;
			AllowedIDs = new List<int>();
			AllowedGroups = new List<string>();
			Z = 0;
		}

		/// <summary>
		/// Checks if a given point is in the region's area
		/// </summary>
		/// <param name="point">Point to check</param>
		/// <returns>Whether the point exists in the region's area</returns>
		public bool InArea(Rectangle point) => InArea(point.X, point.Y);

		/// <summary>
		/// Checks if a given (x, y) coordinate is in the region's area
		/// </summary>
		/// <param name="x">X coordinate to check</param>
		/// <param name="y">Y coordinate to check</param>
		/// <returns>Whether the coordinate exists in the region's area</returns>
		public bool InArea(int x, int y) //overloaded with x,y
=>
			/*
			DO NOT CHANGE TO Area.Contains(x, y)!
			Area.Contains does not account for the right and bottom 'border' of the rectangle,
			which results in regions being trimmed.
			*/
			x >= Area.X && x <= Area.X + Area.Width && y >= Area.Y && y <= Area.Y + Area.Height;

		/// <summary>
		/// Checks if a given player has permission to build in the region
		/// </summary>
		/// <param name="ply">Player to check permissions with</param>
		/// <returns>Whether the player has permission</returns>
		public bool HasPermissionToBuildInRegion(TSPlayer ply)
		{
			if (!DisableBuild)
			{
				return true;
			}
			if (!ply.IsLoggedIn)
			{
				if (!ply.HasBeenNaggedAboutLoggingIn)
				{
					ply.SendMessage(GetString("You must be logged in to take advantage of protected regions."), Color.Red);
					ply.HasBeenNaggedAboutLoggingIn = true;
				}
				return false;
			}

			return ply.HasPermission(Permissions.editregion) || AllowedIDs.Contains(ply.Account.ID) || AllowedGroups.Contains(ply.Group.Name) || Owner == ply.Account.Name;
		}

		/// <summary>
		/// Sets the user IDs which are allowed to use the region
		/// </summary>
		/// <param name="ids">String of IDs to set</param>
		public void SetAllowedIDs(String ids)
		{
			String[] idArr = ids.Split(',');
			List<int> idList = new List<int>();

			foreach (String id in idArr)
			{
				int i = 0;
				if (int.TryParse(id, out i) && i != 0)
				{
					idList.Add(i);
				}
			}
			AllowedIDs = idList;
		}

		/// <summary>
		/// Sets the group names which are allowed to use the region
		/// </summary>
		/// <param name="groups">String of group names to set</param>
		public void SetAllowedGroups(String groups)
		{
			// prevent null pointer exceptions
			if (!string.IsNullOrEmpty(groups))
			{
				List<String> groupList = groups.Split(',').ToList();

				for (int i = 0; i < groupList.Count; i++)
				{
					groupList[i] = groupList[i].Trim();
				}

				AllowedGroups = groupList;
			}
		}

		/// <summary>
		/// Removes a user's access to the region
		/// </summary>
		/// <param name="id">User ID to remove</param>
		/// <returns>true if the user was found and removed from the region's allowed users</returns>
		public bool RemoveID(int id) => AllowedIDs.Remove(id);

		/// <summary>
		/// Removes a group's access to the region
		/// </summary>
		/// <param name="groupName">Group name to remove</param>
		/// <returns></returns>
		public bool RemoveGroup(string groupName) => AllowedGroups.Remove(groupName);
	}
}
