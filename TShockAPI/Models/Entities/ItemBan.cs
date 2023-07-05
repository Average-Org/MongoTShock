using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI.Hooks;
using Entity = MongoDB.Entities.Entity;

namespace TShockAPI
{
	public class ItemBan : Entity, IEquatable<ItemBan>
	{
		public string Name { get; set; } = string.Empty;
		public List<string> AllowedGroups { get; set; }

		public ItemBan(string name)
			: this()
		{
			Name = name;
			AllowedGroups = new List<string>();
		}

		public ItemBan()
		{
			Name = "";
			AllowedGroups = new List<string>();
		}

		public bool Equals(ItemBan other) => Name == other.Name;

		public bool HasPermissionToUseItem(TSPlayer ply)
		{
			if (ply == null)
				return false;

			if (ply.HasPermission(Permissions.usebanneditem))
				return true;

			PermissionHookResult hookResult = PlayerHooks.OnPlayerItembanPermission(ply, this);
			if (hookResult != PermissionHookResult.Unhandled)
				return hookResult == PermissionHookResult.Granted;

			var cur = ply.Group;
			var traversed = new List<Group>();
			while (cur != null)
			{
				if (AllowedGroups.Contains(cur.Name))
				{
					return true;
				}
				if (traversed.Contains(cur))
				{
					throw new InvalidOperationException("Infinite group parenting ({0})".SFormat(cur.Name));
				}
				traversed.Add(cur);
				cur = cur.Parent;
			}
			return false;
			// could add in the other permissions in this class instead of a giant if switch.
		}

		public void SetAllowedGroups(String groups)
		{
			// prevent null pointer exceptions
			if (!string.IsNullOrEmpty(groups))
			{
				List<String> groupArr = groups.Split(',').ToList();

				for (int i = 0; i < groupArr.Count; i++)
				{
					groupArr[i] = groupArr[i].Trim();
					//Console.WriteLine(groupArr[i]);
				}
				AllowedGroups = groupArr;
			}
		}

		public bool RemoveGroup(string groupName) => AllowedGroups.Remove(groupName);

		public override string ToString() => Name + (AllowedGroups.Count > 0 ? " (" + String.Join(",", AllowedGroups) + ")" : "");
	}
}
