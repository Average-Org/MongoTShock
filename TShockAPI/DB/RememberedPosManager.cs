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

using Microsoft.Xna.Framework;
using MongoDB.Entities;
using System.Linq;
using Terraria;
using TShockAPI.Models.Entities;

namespace TShockAPI.DB
{
	public class RememberedPosManager
	{
		public RememberedPosManager() { }

		public Vector2 CheckLeavePos(string name) => MongoDB.Entities.DB.Find<LastPosition>().
			ManyAsync(x => x.Name == name).Result.
			FirstOrDefault().Position;

		public Vector2 GetLeavePos(string name, string IP) => MongoDB.Entities.DB.Find<LastPosition>().
			ManyAsync(x => x.Name == name && x.IP == IP).Result.
			FirstOrDefault().Position;

		public void InsertLeavePos(string name, string IP, int X, int Y)
		{
			var pos = new LastPosition
			{
				Name = name,
				IP = IP,
				X = X,
				Y = Y,
				WorldID = Main.worldID.ToString()
			};
			pos.SaveAsync();
		}
	}
}
