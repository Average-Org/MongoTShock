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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TShockAPI.Hooks;

namespace TShockAPI.DB
{
	public class ItemManager
	{
		public ItemManager() { }

		public void AddNewBan(string itemname = "")
		{
			if (IsBanned(itemname, null)) return;
			ItemBan ban = new()
			{
				Name = itemname
			};
			ban.SaveAsync();
		}

		public void RemoveBan(string itemname)
		{
			if (!IsBanned(itemname, null))
				return;

			ItemBan ban = MongoDB.Entities.DB.Find<ItemBan>().ManyAsync(x => x.Name == itemname).Result.First();
			ban.DeleteAsync();
		}

		public bool IsBanned(string name) => MongoDB.Entities.DB.Find<ItemBan>().ManyAsync(x => x.Name == name).Result.Any();

		public bool IsBanned(string name, TSPlayer ply)
		{
			ItemBan b = GetItemBanByName(name);
			return b != null &&!b.HasPermissionToUseItem(ply);
		}

		public IEnumerable<ItemBan> RetrieveAll() => MongoDB.Entities.DB.Find<ItemBan>().Match(w => true).ExecuteAsync().Result;

		public bool AllowGroup(string item, string name)
		{
			ItemBan ban = GetItemBanByName(item);
			if (ban != null)
			{
				if(ban.AllowedGroups.Contains(name)) return true;
				ban.AllowedGroups.Add(name);
				ban.SaveAsync();
			}

			return false;
		}

		public bool RemoveGroup(string item, string group)
		{
			ItemBan ban = GetItemBanByName(item);
			if (ban is null) return false;
			
			ban.AllowedGroups.Remove(group);
			ban.SaveAsync();
			return true;
		}

		public ItemBan GetItemBanByName(string name) => MongoDB.Entities.DB.Find<ItemBan>().ManyAsync(x => x.Name == name).Result.FirstOrDefault();
	}

	
}
