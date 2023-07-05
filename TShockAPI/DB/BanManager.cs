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
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Collections.ObjectModel;
using db = MongoDB.Entities.DB;
using MongoDB.Entities;

namespace TShockAPI.DB
{
	/// <summary>
	/// Class that manages bans.
	/// </summary>
	public class BanManager
	{
		/// <summary>
		/// Event invoked when a ban is checked for validity
		/// </summary>
		public static event EventHandler<BanEventArgs> OnBanValidate;
		/// <summary>
		/// Event invoked before a ban is added
		/// </summary>
		public static event EventHandler<BanPreAddEventArgs> OnBanPreAdd;
		/// <summary>
		/// Event invoked after a ban is added
		/// </summary>
		public static event EventHandler<BanEventArgs> OnBanPostAdd;

		/// <summary>
		/// Initializes a new instance of the <see cref="TShockAPI.DB.BanManager"/> class.
		/// </summary>
		/// <param name="db">A valid connection to the TShock database</param>
		public BanManager()
		{
			OnBanValidate += BanValidateCheck;
		}

		internal bool CheckBan(TSPlayer player)
		{
			Ban ban = null;
			if (player != null)
			{
				if (db.Find<Ban>().ManyAsync(x => x.IP == player.IP && x.Active == true).Result.Any()
					|| db.Find<Ban>().ManyAsync(x => x.UUID == player.UUID && x.Active == true).Result.Any()
					|| db.Find<Ban>().ManyAsync(x => x.AccountName == player.Account.Name && x.Active == true).Result.Any())
				{
					if (ban.Expires is null)
					{
						player.Disconnect(GetParticularString("{0} is ban number, {1} is ban reason", $"#{ban.ID} - You are banned: {ban.Reason}"));
						return true;
					}
					else
					{
						TimeSpan ts = (TimeSpan)(ban.Expires - DateTime.UtcNow);
						player.Disconnect(GetParticularString("{0} is ban number, {1} is ban reason, {2} is a timestamp", $"#{ban.ID} - You are banned: {ban.Reason} ({ban.GetPrettyExpirationString()} remaining)"));
						return true;
					}
				}
			}


			return false;
		}

		/// <summary>
		/// Determines whether or not a ban is valid
		/// </summary>
		/// <param name="ban"></param>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool IsValidBan(Ban ban, TSPlayer player)
		{
			BanEventArgs args = new BanEventArgs
			{
				Ban = ban,
				Player = player
			};

			OnBanValidate?.Invoke(this, args);

			return args.Valid;
		}

		internal void BanValidateCheck(object sender, BanEventArgs args)
		{
			//Only perform validation if the event has not been cancelled before we got here
			if (args.Valid)
			{
				//We consider a ban to be valid if the start time is before now and the end time is after now
				args.Valid = (DateTime.UtcNow > args.Ban.TimeBanned && DateTime.UtcNow < args.Ban.Expires);
			}
		}

		/// <summary>
		/// Adds a new ban for the given identifier. Returns a Ban object if the ban was added, else null
		/// </summary>
		/// <param name="identifier"></param>
		/// <param name="reason"></param>
		/// <param name="banningUser"></param>
		/// <param name="fromDate"></param>
		/// <param name="toDate"></param>
		/// <returns></returns>
		public Ban InsertBan(string accountName, string reason, string banningUser, DateTime fromDate, DateTime toDate)
		{
			Ban ban = new()
			{
				AccountName = accountName,
				Reason = reason,
				BannedBy = banningUser,
				TimeBanned = fromDate,
				Expires = toDate,
			};
			ban.SaveAsync();
			return ban;
		}

		public Ban InsertBan(string identifier, string reason, string banningUser, DateTime fromDate, DateTime toDate, BanType banType = BanType.AccountName)
		{
			switch (banType)
			{
				case BanType.UUID:
					{
						Ban ban = new()
						{
							UUID = identifier,
							Reason = reason,
							BannedBy = banningUser,
							TimeBanned = fromDate,
							Expires = toDate,
						};
						ban.SaveAsync();
						return ban;
					}
				case BanType.IP:
					{
						Ban ban = new()
						{
							IP = identifier,
							Reason = reason,
							BannedBy = banningUser,
							TimeBanned = fromDate,
							Expires = toDate,
						};
						ban.SaveAsync();
						return ban;
					}
				default:
				case BanType.AccountName:
					{
						Ban ban = new()
						{
							AccountName = identifier,
							Reason = reason,
							BannedBy = banningUser,
							TimeBanned = fromDate,
							Expires = toDate,
						};
						ban.SaveAsync();
						return ban;
					}
			}

		}

		public enum BanType
		{
			IP,
			UUID,
			AccountName
		}

	

		/// <summary>
		/// Attempts to remove a ban. Returns true if the ban was removed or expired. False if the ban could not be removed or expired
		/// </summary>
		/// <param name="ticketNumber">The ticket number of the ban to change</param>
		/// <param name="fullDelete">If true, deletes the ban from the database. If false, marks the expiration time as now, rendering the ban expired. Defaults to false</param>
		/// <returns></returns>
		public bool RemoveBan(string id, bool fullDelete = false)
		=> !db.Find<Ban>().ManyAsync(x => x.ID == id).Result.First().DeleteAsync().IsFaulted;


		/// <summary>
		/// Retrieves a single ban from a ban's ticket number
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Ban GetBanById(string id) => db.Find<Ban>().ManyAsync(x => x.ID == id).Result.First();

		public IEnumerable<Ban> RetrieveBanByAccount(string accName, bool currentOnly = true) => db.Find<Ban>().ManyAsync(x => x.AccountName == accName && x.Active == true).Result;

		public IEnumerable<Ban> RetrieveBanByIP(string ip, bool currentOnly = true) => db.Find<Ban>().ManyAsync(x => x.IP == ip && x.Active == true).Result;

		public IEnumerable<Ban> RetrieveBanByUUID(string uuid, bool currentOnly = true) => db.Find<Ban>().ManyAsync(x => x.UUID == uuid && x.Active == true).Result;


		/// <summary>
		/// Retrieves a list of bans from the database, sorted by their addition date from newest to oldest
		/// </summary>
		public IEnumerable<Ban> RetrieveAllBans() => RetrieveAllBansSorted(BanSortMethod.AddedNewestToOldest);

		/// <summary>
		/// Retrieves an enumerable of <see cref="Ban"/>s from the database, sorted using the provided sort method
		/// </summary>
		/// <param name="sortMethod"></param>
		/// <returns></returns>
		public IEnumerable<Ban> RetrieveAllBansSorted(BanSortMethod sortMethod)
		{
			List<Ban> banlist = new List<Ban>();
			switch (sortMethod)
			{
				case BanSortMethod.ExpirationSoonestToLatest:
					{
						banlist = db.Find<Ban>().ManyAsync(x => x.Active == true).Result.OrderBy(x => x.Expires).ToList();
						break;
					}
				case BanSortMethod.ExpirationLatestToSoonest:
					{
						banlist = db.Find<Ban>().ManyAsync(x => x.Active == true).Result.OrderByDescending(x => x.Expires).ToList();
						break;
					}
				case BanSortMethod.AddedNewestToOldest:
					{
						banlist = db.Find<Ban>().ManyAsync(x => x.Active == true).Result.OrderByDescending(x => x.TimeBanned).ToList();
						break;
					}
				case BanSortMethod.AddedOldestToNewest:
					{
						banlist = db.Find<Ban>().ManyAsync(x => x.Active == true).Result.OrderBy(x => x.TimeBanned).ToList();
						break;
					}
			}

			return banlist;
		}

		/// <summary>
		/// Removes all bans from the database
		/// </summary>
		public void ClearBans() => db.Find<Ban>().ManyAsync(x => x.Active == true).Result.ForEach(x => x.DeleteAsync());

		/// <summary>
		/// Enum containing sort options for ban retrieval
		/// </summary>
		public enum BanSortMethod
		{
			/// <summary>
			/// Bans will be sorted on expiration date, from soonest to latest
			/// </summary>
			ExpirationSoonestToLatest,
			/// <summary>
			/// Bans will be sorted on expiration date, from latest to soonest
			/// </summary>
			ExpirationLatestToSoonest,
			/// <summary>
			/// Bans will be sorted by the date they were added, from newest to oldest
			/// </summary>
			AddedNewestToOldest,
			/// <summary>
			/// Bans will be sorted by the date they were added, from oldest to newest
			/// </summary>
			AddedOldestToNewest,
		}

		/// <summary>
		/// Result of an attempt to add a ban
		/// </summary>
		public class AddBanResult
		{
			/// <summary>
			/// Message generated from the attempt
			/// </summary>
			public string Message { get; set; }
			/// <summary>
			/// Ban object generated from the attempt, or null if the attempt failed
			/// </summary>
			public Ban Ban { get; set; }
		}

		/// <summary>
		/// Event args used for completed bans
		/// </summary>
		public class BanEventArgs : EventArgs
		{
			/// <summary>
			/// Complete ban object
			/// </summary>
			public Ban Ban { get; set; }

			/// <summary>
			/// Player ban is being applied to
			/// </summary>
			public TSPlayer Player { get; set; }

			/// <summary>
			/// Whether or not the operation should be considered to be valid
			/// </summary>
			public bool Valid { get; set; } = true;
		}

		/// <summary>
		/// Event args used for ban data prior to a ban being formalized
		/// </summary>
		public class BanPreAddEventArgs : EventArgs
		{
			/// <summary>
			/// An identifiable piece of information to ban
			/// </summary>
			public string Identifier { get; set; }

			/// <summary>
			/// Gets or sets the ban reason.
			/// </summary>
			/// <value>The ban reason.</value>
			public string Reason { get; set; }

			/// <summary>
			/// Gets or sets the name of the user who added this ban entry.
			/// </summary>
			/// <value>The banning user.</value>
			public string BanningUser { get; set; }

			/// <summary>
			/// DateTime from which the ban will take effect
			/// </summary>
			public DateTime TimeBanned { get; set; }

			/// <summary>
			/// DateTime at which the ban will end
			/// </summary>
			public DateTime Expires { get; set; }

			/// <summary>
			/// Whether or not the operation should be considered to be valid
			/// </summary>
			public bool Valid { get; set; } = true;

			/// <summary>
			/// Optional message to explain why the event was invalidated, if it was
			/// </summary>
			public string Message { get; set; }
		}

	}

}
