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
using System.Linq;

namespace TShockAPI
{
	public class TileManager
	{
		public TileManager() { }

		public void AddNewBan(short id = 0)
		{
			if (TileIsBanned(id))
				return;

			TileBan ban = new()
			{
				ID = id
			};
			ban.SaveAsync();

		}

		public void RemoveBan(short id)
		{
			if (!TileIsBanned(id, null))
				return;
		}

		public bool TileIsBanned(short id) => MongoDB.Entities.DB.Find<TileBan>().ManyAsync(x => x.ID == id).Result.Any();


		public bool TileIsBanned(short id, TSPlayer ply)
		{
			if (TileIsBanned(id))
			{
				TileBan b = GetBanById(id);
				return !b.HasPermissionToPlaceTile(ply);
			}
			return false;
		}

		public bool AllowGroup(short id, string name)
		{
			TileBan b = GetBanById(id);
			if (!b.AllowedGroups.Contains(name))
			{
				b.AllowedGroups.Add(name);
				b.SaveAsync();
				return true;
			}
			return false;
		}

		public bool RemoveGroup(short id, string group)
		{
			TileBan b = GetBanById(id);
			if (b != null)
			{
				b.AllowedGroups.RemoveAll(x => x == group);
				b.SaveAsync();
				return true;
			}
			return false;
		}

		public TileBan GetBanById(short id) => MongoDB.Entities.DB.Find<TileBan>().ManyAsync(x => x.ID == id).Result.FirstOrDefault();

	}


}
