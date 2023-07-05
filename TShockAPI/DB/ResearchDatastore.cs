using MongoDB.Entities;
using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI.Models.Entities;

namespace TShockAPI.DB
{
	/// <summary>
	/// This class is used as the data interface for Journey mode research.
	/// This information is maintained such that SSC characters will be properly set up with
	/// the world's current research.
	/// </summary>
	public class ResearchDatastore
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TShockAPI.DB.ResearchDatastore"/> class.
		/// </summary>
		/// <param name="db">A valid connection to the TShock database</param>
		public ResearchDatastore() { }

		/// <summary>
		/// This call will return the memory-cached list of items sacrificed.
		/// If the cache is not initialized, it will be initialized from the database.
		/// </summary>
		/// <returns></returns>
		public Dictionary<int,int> GetSacrificedItems() => ReadFromDatabase();
		

		/// <summary>
		/// This function will return a Dictionary&lt;ItemId, AmountSacrificed&gt; representing
		/// what the progress of research on items is for this world.
		/// </summary>
		/// <returns>A dictionary of ItemID keys and Amount Sacrificed values.</returns>
		private Dictionary<int, int> ReadFromDatabase()
		{
			Dictionary<int, int> sacrificedItems = new Dictionary<int, int>();

			MongoDB.Entities.DB.Find<Research>().ManyAsync(x => true).Result.ForEach(x =>
			{
				if (sacrificedItems.ContainsKey(x.ItemId))
				{
					sacrificedItems[x.ItemId] += x.AmountSacrificed;
				}
				else
				{
					sacrificedItems.Add(x.ItemId, x.AmountSacrificed);
				}
			});


			return sacrificedItems;
		}

		/// <summary>
		/// This method will sacrifice an amount of an item for research.
		/// </summary>
		/// <param name="itemId">The net ItemId that is being researched.</param>
		/// <param name="amount">The amount of items being sacrificed.</param>
		/// <param name="player">The player who sacrificed the item for research.</param>
		/// <returns>The cumulative total sacrifices for this item.</returns>
		public int SacrificeItem(int itemId, int amount, TSPlayer player)
		{
			var itemsSacrificed = GetSacrificedItems();
			if (!itemsSacrificed.ContainsKey(itemId))
			{
				Research sacrifice = new()
				{
					WorldId = Main.worldID,
					PlayerId = player.Account.ID,
					ItemId = itemId,
					AmountSacrificed = amount,
					TimeSacrificed = DateTime.Now
				};
				sacrifice.SaveAsync();
			}
			

			return itemsSacrificed[itemId];
		}
	}
}
