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
using System.Linq;
using System.Text;
using Terraria;
using MongoDB.Entities;
using DB = MongoDB.Entities;

namespace TShockAPI.DB
{
	public class CharacterManager
	{
		public PlayerData GetPlayerData(TSPlayer player, int acctid)
		{
			PlayerData playerData = new PlayerData(player);

			var data = MongoDB.Entities.DB.Find<PlayerData>().ManyAsync(a => a.Account == acctid).Result.FirstOrDefault();
			if (data is null)
				return null;

			playerData.Exists = true;
			playerData.Health = data.Health;
			playerData.MaxHealth = data.MaxHealth;
			playerData.Mana = data.Mana;
			playerData.MaxMana = data.MaxMana;
			var inv = data.Inventory.ToList();

			if(inv.Count < NetItem.MaxInventory)
			{
				//TODO: unhardcode this - stop using magic numbers and use NetItem numbers
				//Set new armour slots empty
				inv.InsertRange(67, new NetItem[2]);
				//Set new vanity slots empty
				inv.InsertRange(77, new NetItem[2]);
				//Set new dye slots empty
				inv.InsertRange(87, new NetItem[2]);
				//Set the rest of the new slots empty
				inv.AddRange(new NetItem[NetItem.MaxInventory - inv.Count]);
			}
			playerData.Inventory = inv.ToArray();
			playerData.ExtraSlot = data.ExtraSlot;
			playerData.SpawnX = data.SpawnX;
			playerData.SpawnY = data.SpawnY;
			playerData.SkinVariant = data.SkinVariant;
			playerData.Hair = data.Hair;
			playerData.HairDye = data.HairDye;

			return playerData;
		}

		public PlayerData SeedInitialData(UserAccount account)
		{
			var inventory = new StringBuilder();

			var items = new List<NetItem>(TShock.ServerSideCharacterConfig.Settings.StartingInventory);
			if (items.Count < NetItem.MaxInventory)
				items.AddRange(new NetItem[NetItem.MaxInventory - items.Count]);
			
			PlayerData data = new PlayerData()
			{
				Account = account.ID,
				Health = TShock.ServerSideCharacterConfig.Settings.StartingHealth,
				MaxHealth = TShock.ServerSideCharacterConfig.Settings.StartingHealth,
				Mana = TShock.ServerSideCharacterConfig.Settings.StartingMana,
				MaxMana = TShock.ServerSideCharacterConfig.Settings.StartingMana,
				Inventory = items.ToArray(),
				SpawnX = -1,
				SpawnY = -1,
				QuestsCompleted = 0
			};

			data.SaveAsync();
			return data;
		}

		/// <summary>
		/// Inserts player data to the tsCharacter database table
		/// </summary>
		/// <param name="player">player to take data from</param>
		/// <returns>true if inserted successfully</returns>
		public bool InsertPlayerData(TSPlayer player, bool fromCommand = false)
		{
			PlayerData playerData = player.PlayerData;

			if (!player.IsLoggedIn || player.State < 10)
				return false;

			if (player.HasPermission(Permissions.bypassssc) && !fromCommand)
			{
				TShock.Log.ConsoleInfo(GetParticularString("{0} is a player name", $"Skipping SSC save (due to tshock.ignore.ssc) for {player.Account.Name}"));
				return false;
			}

			if (playerData is null)
				return false;

			var data = MongoDB.Entities.DB.Find<PlayerData>().ManyAsync(a => a.Account == player.Account.ID).Result.FirstOrDefault();
			data = data ?? SeedInitialData(player.Account);
			data.SaveAsync();
			return true;
		}

		/// <summary>
		/// Removes a player's data from the tsCharacter database table
		/// </summary>
		/// <param name="userid">User ID of the player</param>
		/// <returns>true if removed successfully</returns>
		public bool RemovePlayer(int userid)
		{
			try
			{
				var data = MongoDB.Entities.DB.Find<PlayerData>().ManyAsync(a => a.Account == userid).Result.FirstOrDefault();
				data.DeleteAsync();
				return true;
			}
			catch (Exception ex)
			{
				TShock.Log.Error(ex.ToString());
			}

			return false;
		}

		/// <summary>
		/// Inserts a specific PlayerData into the SSC table for a player.
		/// </summary>
		/// <param name="player">The player to store the data for.</param>
		/// <param name="data">The player data to store.</param>
		/// <returns>If the command succeeds.</returns>
		[Obsolete("Use InsertPlayerData(TSPlayer player) instead.")]
		public bool InsertSpecificPlayerData(TSPlayer player, PlayerData data)
		{
			InsertPlayerData(player);
			return true;
		}
	}
}
