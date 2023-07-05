using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI.Hooks;
using Entity = MongoDB.Entities.Entity;

namespace TShockAPI
{
	public class TileBan : Entity, IEquatable<TileBan>
	{
		public short ID { get; set; }
		public List<string> AllowedGroups { get; set; }

		public TileBan(short id)
			: this()
		{
			ID = id;
			AllowedGroups = new List<string>();
		}

		public TileBan()
		{
			ID = 0;
			AllowedGroups = new List<string>();
		}

		public bool Equals(TileBan other) => ID == other.ID;

		public bool HasPermissionToPlaceTile(TSPlayer ply)
		{
			if (ply == null)
				return false;

			if (ply.HasPermission(Permissions.canusebannedtiles))
				return true;

			PermissionHookResult hookResult = PlayerHooks.OnPlayerTilebanPermission(ply, this);
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
					throw new InvalidOperationException(GetString($"Infinite group parenting ({cur.Name})"));
				}
				traversed.Add(cur);
				cur = cur.Parent;
				this.SaveAsync();

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
				this.SaveAsync();
			}
		}

		public bool RemoveGroup(string groupName) => AllowedGroups.Remove(groupName);

		public override string ToString() => ID + (AllowedGroups.Count > 0 ? " (" + String.Join(",", AllowedGroups) + ")" : "");
	}
}
