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

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;
using Microsoft.Xna.Framework;
using MongoDB.Entities;
using db = MongoDB.Entities.DB;


namespace TShockAPI.DB
{
	public class WarpManager
	{
		/// <summary>
		/// Adds a warp.
		/// </summary>
		/// <param name="x">The X position.</param>
		/// <param name="y">The Y position.</param>
		/// <param name="name">The name.</param>
		/// <returns>Whether the operation succeeded.</returns>
		public bool Add(int x, int y, string name)
		{
			Warp warp = new()
			{
				Position = new(x, y),
				Name = name,
				WorldID = Main.worldID.ToString()
			};

			warp.SaveAsync();
			return true;
		}

		public IEnumerable<Warp> RetrieveAll() => db.Find<Warp>().Match(w => w.WorldID == Main.worldID.ToString()).ExecuteAsync().Result;

		/// <summary>
		/// Removes a warp.
		/// </summary>
		/// <param name="warpName">The warp name.</param>
		/// <returns>Whether the operation succeeded.</returns>
		public bool Remove(string warpName)
		{
			try { db.Find<Warp>().ManyAsync(x => x.Name == warpName).Result.ForEach(x => x.DeleteAsync()); return true; }
			catch { return false; }
		}

		/// <summary>
		/// Finds the warp with the given name.
		/// </summary>
		/// <param name="warpName">The name.</param>
		/// <returns>The warp, if it exists, or else null.</returns>
		public Warp Find(string warpName) => db.Find<Warp>().ManyAsync(x => x.Name == warpName).Result.FirstOrDefault();

		/// <summary>
		/// Sets the position of a warp.
		/// </summary>
		/// <param name="warpName">The warp name.</param>
		/// <param name="x">The X position.</param>
		/// <param name="y">The Y position.</param>
		/// <returns>Whether the operation succeeded.</returns>
		public void Position(string warpName, int x, int y)
		{
			Warp warp = db.Find<Warp>().ManyAsync(x => x.Name == warpName).Result.FirstOrDefault();
			warp.Position = new(x, y);
			warp.SaveAsync();
		}

		/// <summary>
		/// Sets the hidden state of a warp.
		/// </summary>
		/// <param name="warpName">The warp name.</param>
		/// <param name="state">The state.</param>
		/// <returns>Whether the operation succeeded.</returns>
		public bool Hide(string warpName, bool state)
		{
			Warp warp = Find(warpName);
			warp.IsPrivate = state;
			return true;
		}
	}


}
