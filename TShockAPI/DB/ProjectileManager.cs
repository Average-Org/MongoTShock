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

namespace TShockAPI.DB
{
	public class ProjectileManagager
	{
		public ProjectileManagager() { }

		public void AddNewBan(short id = 0)
		{
			if (IsBanned(id, null)) return;
			ProjectileBan ban = new()
			{
				ID = id
			};
			ban.SaveAsync();
		}

		public void RemoveBan(short id)
		{
			if (!IsBanned(id, null))
				return;
			ProjectileBan ban = GetBanById(id);
			ban.DeleteAsync();
		}

		public bool IsBanned(short id) => MongoDB.Entities.DB.Find<ProjectileBan>().ManyAsync(x => x.ID == id).Result.Any();

		public bool IsBanned(short id, TSPlayer ply)
		{
			ProjectileBan b = GetBanById(id);
			return !b.HasPermissionToCreateProjectile(ply);
		}

		public bool AllowGroup(short id, string name)
		{
			ProjectileBan ban = GetBanById(id);
			if (ban != null)
			{
				if (ban.AllowedGroups.Contains(name)) return true;
				ban.AllowedGroups.Add(name);
				ban.SaveAsync();
			}

			return false;
		}

		public bool RemoveGroup(short id, string group)
		{
			ProjectileBan ban = GetBanById(id);
			if (ban is null) return false;

			ban.AllowedGroups.Remove(group);
			ban.SaveAsync();
			return true;
		}

		public ProjectileBan GetBanById(short id) => MongoDB.Entities.DB.Find<ProjectileBan>().ManyAsync(x => x.ID == id).Result.FirstOrDefault();

	}

}
