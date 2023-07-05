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
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using Terraria;

namespace TShockAPI.DB
{
	/// <summary>
	/// Represents the Region database manager.
	/// </summary>
	public class RegionManager
	{
		internal RegionManager() { }

		/// <summary>
		/// Reloads all regions.
		/// </summary>
		[Obsolete("This is no longer necessary circa MongoDB.")]
		public void Reload()
		{
			// no longer necessary
		}

		/// <summary>
		/// Adds a region to the database.
		/// </summary>
		/// <param name="tx">TileX of the top left corner.</param>
		/// <param name="ty">TileY of the top left corner.</param>
		/// <param name="width">Width of the region in tiles.</param>
		/// <param name="height">Height of the region in tiles.</param>
		/// <param name="regionname">The name of the region.</param>
		/// <param name="owner">The User Account Name of the person who created this region.</param>
		/// <param name="worldid">The world id that this region is in.</param>
		/// <param name="z">The Z index of the region.</param>
		/// <returns>Whether the region was created and added successfully.</returns>
		public bool AddRegion(int tx, int ty, int width, int height, string regionname, string owner, string worldid, int z = 0)
		{
			if (GetRegionByName(regionname) != null)
				return false;


			Region region = new Region()
			{
				Area = new Microsoft.Xna.Framework.Rectangle(tx, ty, width, height),
				Name = regionname,
				WorldID = worldid,
				Owner = owner,
				DisableBuild = true,
				Z = z
			};

			Hooks.RegionHooks.OnRegionCreated(region);
			return true;

		}

		/// <summary>
		/// Deletes the region from this world with a given ID.
		/// </summary>
		/// <param name="id">The ID of the region to delete.</param>
		/// <returns>Whether the region was successfully deleted.</returns>
		public bool DeleteRegion(int id)
		{
			var region = GetRegionByID(id);
			Hooks.RegionHooks.OnRegionDeleted(region);
			region.DeleteAsync();
			return true;
		}

		/// <summary>
		/// Deletes the region from this world with a given name.
		/// </summary>
		/// <param name="name">The name of the region to delete.</param>
		/// <returns>Whether the region was successfully deleted.</returns>
		public bool DeleteRegion(string name)
		{
			var worldId = Main.worldID;
			var region = GetRegionByName(name);
			Hooks.RegionHooks.OnRegionDeleted(region);
			region.DeleteAsync();
			return true;
		}

		/// <summary>
		/// Sets the protected state of the region with a given ID.
		/// </summary>
		/// <param name="id">The ID of the region to change.</param>
		/// <param name="state">New protected state of the region.</param>
		/// <returns>Whether the region's state was successfully changed.</returns>
		public bool SetRegionState(int id, bool state)
		{
			Region region = GetRegionByID(id);
			if(region is not null)
				region.DisableBuild = state ? true : false;
			return true;
		}

		public IEnumerable<Region> RetrieveAll() => MongoDB.Entities.DB.Find<Region>().Match(w => w.WorldID==Main.worldID.ToString()).ExecuteAsync().Result;


		/// <summary>
		/// Sets the protected state of the region with a given name.
		/// </summary>
		/// <param name="name">The name of the region to change.</param>
		/// <param name="state">New protected state of the region.</param>
		/// <returns>Whether the region's state was successfully changed.</returns>
		public bool SetRegionState(string name, bool state)
		{
			Region region = GetRegionByName(name);
			if (region is not null)
				region.DisableBuild = state ? true : false;
			return true;
		}

		/// <summary>
		/// Checks if a given player can build in a region at the given (x, y) coordinate
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="ply">Player to check permissions with</param>
		/// <returns>Whether the player can build at the given (x, y) coordinate</returns>
		public bool CanBuild(int x, int y, TSPlayer ply)
		{
			if (!ply.HasPermission(Permissions.canbuild))
				return false;
			Region top = null;

			Region region = MongoDB.Entities.DB.Find<Region>().
				ManyAsync(r => ply.X >= r.Area.X && ply.X <= r.Area.X + r.Area.Width && ply.Y >= r.Area.Y && ply.Y <= r.Area.Y + r.Area.Height)
				.Result.First();

			if (region is null)
				return true;

			return region.HasPermissionToBuildInRegion(ply);
		}

		/// <summary>
		/// Checks if any regions exist at the given (x, y) coordinate
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>Whether any regions exist at the given (x, y) coordinate</returns>
		public bool InArea(int x, int y) => MongoDB.Entities.DB.Find<Region>().
				ManyAsync(r => x >= r.Area.X && x <= r.Area.X + r.Area.Width && y >= r.Area.Y && y <= r.Area.Y + r.Area.Height)
				.Result.Any();

		/// <summary>
		/// Checks if any regions exist at the given (x, y) coordinate
		/// and returns an IEnumerable containing their names
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>The names of any regions that exist at the given (x, y) coordinate</returns>
		public IEnumerable<string> InAreaRegionName(int x, int y) => MongoDB.Entities.DB.Find<Region>().
				ManyAsync(r => x >= r.Area.X && x <= r.Area.X + r.Area.Width && y >= r.Area.Y && y <= r.Area.Y + r.Area.Height)
				.Result.Select(r => r.Name);

		/// <summary>
		/// Checks if any regions exist at the given (x, y) coordinate
		/// and returns an IEnumerable containing their IDs
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>The IDs of any regions that exist at the given (x, y) coordinate</returns>
		public IEnumerable<int> InAreaRegionID(int x, int y) => MongoDB.Entities.DB.Find<Region>().
				ManyAsync(r => x >= r.Area.X && x <= r.Area.X + r.Area.Width && y >= r.Area.Y && y <= r.Area.Y + r.Area.Height)
				.Result.Select(r => r.ID);

		/// <summary>
		/// Checks if any regions exist at the given (x, y) coordinate
		/// and returns an IEnumerable containing their <see cref="Region"/> objects
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>The <see cref="Region"/> objects of any regions that exist at the given (x, y) coordinate</returns>
		public IEnumerable<Region> InAreaRegion(int x, int y) => MongoDB.Entities.DB.Find<Region>().
				ManyAsync(r => x >= r.Area.X && x <= r.Area.X + r.Area.Width && y >= r.Area.Y && y <= r.Area.Y + r.Area.Height)
				.Result;

		/// <summary>
		/// Changes the size of a given region
		/// </summary>
		/// <param name="regionName">Name of the region to resize</param>
		/// <param name="addAmount">Amount to resize</param>
		/// <param name="direction">Direction to resize in:
		/// 0 = resize height and Y.
		/// 1 = resize width.
		/// 2 = resize height.
		/// 3 = resize width and X.</param>
		/// <returns></returns>
		public bool ResizeRegion(string regionName, int addAmount, int direction)
		{

			Region region = GetRegionByName(regionName);
			if(region is null)
				return false;


			int X = region.Area.X;
			int Y = region.Area.Y;
			int height = region.Area.Height;
			int width = region.Area.Width;

			//0 = up
			//1 = right
			//2 = down
			//3 = left
			switch (direction)
				{
					case 0:
						Y -= addAmount;
						height += addAmount;
						break;
					case 1:
						width += addAmount;
						break;
					case 2:
						height += addAmount;
						break;
					case 3:
						X -= addAmount;
						width += addAmount;
						break;
					default:
						return false;
				}

			region.Area = new Microsoft.Xna.Framework.Rectangle(X, Y, width, height);
			region.SaveAsync();
			return true;

			}

		/// <summary>
		/// Renames a region
		/// </summary>
		/// <param name="oldName">Name of the region to rename</param>
		/// <param name="newName">New name of the region</param>
		/// <returns>true if renamed successfully, false otherwise</returns>
		public bool RenameRegion(string oldName, string newName)
		{
			Region region = GetRegionByName(oldName, Main.worldID.ToString());
			if (region is null)
				return false;

			region.Name = newName;
			region.SaveAsync();
			return true;
		}

		/// <summary>
		/// Removes an allowed user from a region
		/// </summary>
		/// <param name="regionName">Name of the region to modify</param>
		/// <param name="userName">Username to remove</param>
		/// <returns>true if removed successfully</returns>
		public bool RemoveUser(string regionName, string userName)
		{
			Region r = GetRegionByName(regionName);
			if (r != null)
			{
				string ids = string.Join(",", r.AllowedIDs);
				r.AllowedIDs.RemoveAll(x=>x==TShock.UserAccounts.GetUserAccountID(userName));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Adds a user to a region's allowed user list
		/// </summary>
		/// <param name="regionName">Name of the region to modify</param>
		/// <param name="userName">Username to add</param>
		/// <returns>true if added successfully</returns>
		public bool AddNewUser(string regionName, string userName)
		{
			Region r = GetRegionByName(regionName);
			if (r != null)
			{
				if (r.AllowedIDs.Contains(TShock.UserAccounts.GetUserAccountID(userName)))
					return false;
				
				r.AllowedIDs.Add(TShock.UserAccounts.GetUserAccountID(userName));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Sets the position of a region.
		/// </summary>
		/// <param name="regionName">The region name.</param>
		/// <param name="x">The X position.</param>
		/// <param name="y">The Y position.</param>
		/// <param name="height">The height.</param>
		/// <param name="width">The width.</param>
		/// <returns>Whether the operation succeeded.</returns>
		public bool PositionRegion(string regionName, int x, int y, int width, int height)
		{
			Region r = GetRegionByName(regionName);
			if (r is not null)
			{
				r.Area = new Microsoft.Xna.Framework.Rectangle(x, y, width, height);
				r.SaveAsync();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets all the regions names from world
		/// </summary>
		/// <param name="worldid">World name to get regions from</param>
		/// <returns>List of regions with only their names</returns>
		public List<Region> ListAllRegions(string worldid) => MongoDB.Entities.DB.Find<Region>().ManyAsync(r => r.WorldID == worldid).Result;

		/// <summary>
		/// Returns a region with the given name
		/// </summary>
		/// <param name="name">Region name</param>
		/// <returns>The region with the given name, or null if not found</returns>
		public Region GetRegionByName(string name) => MongoDB.Entities.DB.Find<Region>().ManyAsync(x => x.WorldID == Main.worldID.ToString() && name == x.Name).Result.FirstOrDefault();

		public Region GetRegionByName(string name, string worldID) => MongoDB.Entities.DB.Find<Region>().ManyAsync(x => x.WorldID == worldID && name == x.Name).Result.FirstOrDefault();


		/// <summary>
		/// Returns a region with the given ID
		/// </summary>
		/// <param name="id">Region ID</param>
		/// <returns>The region with the given ID, or null if not found</returns>
		public Region GetRegionByID(int id) => MongoDB.Entities.DB.Find<Region>().ManyAsync(x => x.WorldID == Main.worldID.ToString() && id == x.ID).Result.FirstOrDefault();

		/// <summary>
		/// Changes the owner of the region with the given name
		/// </summary>
		/// <param name="regionName">Region name</param>
		/// <param name="newOwner">New owner's username</param>
		/// <returns>Whether the change was successful</returns>
		public bool ChangeOwner(string regionName, string newOwner)
		{
			var region = GetRegionByName(regionName);
			if (region != null)
			{
				region.Owner = newOwner;
				region.SaveAsync();
			}
			return false;
		}

		/// <summary>
		/// Allows a group to use a region
		/// </summary>
		/// <param name="regionName">Region name</param>
		/// <param name="groupName">Group's name</param>
		/// <returns>Whether the change was successful</returns>
		public bool AllowGroup(string regionName, string groupName)
		{

			Region r = GetRegionByName(regionName);
			if (r is not null) {
				r.AllowedGroups.Add(groupName);
				r.SaveAsync();
				return true;
			}

			return false;

		}

		/// <summary>
		/// Removes a group's access to a region
		/// </summary>
		/// <param name="regionName">Region name</param>
		/// <param name="group">Group name</param>
		/// <returns>Whether the change was successful</returns>
		public bool RemoveGroup(string regionName, string group)
		{
			Region r = GetRegionByName(regionName);
			if (r is not null)
			{
				r.AllowedGroups.RemoveAll(x => x == group);
				r.SaveAsync();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns the <see cref="Region"/> with the highest Z index of the given list
		/// </summary>
		/// <param name="regions">List of Regions to compare</param>
		/// <returns></returns>
		public Region GetTopRegion(IEnumerable<Region> regions)
		{
			Region ret = null;
			foreach (Region r in regions)
			{
				if (ret == null)
					ret = r;
				else
				{
					if (r.Z > ret.Z)
						ret = r;
				}
			}
			return ret;
		}

		/// <summary>
		/// Sets the Z index of a given region
		/// </summary>
		/// <param name="name">Region name</param>
		/// <param name="z">New Z index</param>
		/// <returns>Whether the change was successful</returns>
		public bool SetZ(string name, int z)
		{
			Region region = GetRegionByName(name);
			region.Z = z;
			region.SaveAsync();
			return true;
		}
	}


}
