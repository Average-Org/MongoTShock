using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Collections.ObjectModel;

namespace TShockAPI
{
	public class Ban : Entity
	{
		public string IP { get; set; }
		public string AccountName { get; set; }
		public string UUID { get; set; }
		public DateTime TimeBanned { get; set; }
		public string BannedBy { get; set; }
		public string Reason { get; set; }
		public bool Active => (Expires is null || Expires > DateTime.UtcNow);
		public DateTime? Expires { get; set; }

		/// <summary>
		/// Returns a string in the format dd:mm:hh:ss indicating the time until the ban expires.
		/// If the ban is not set to expire (ExpirationDateTime == DateTime.MaxValue), returns the string 'Never'
		/// </summary>
		/// <returns></returns>
		public string GetPrettyExpirationString()
		{
			if (Expires is null)
				return "never";

			TimeSpan ts = ((DateTime)Expires - DateTime.UtcNow).Duration(); // Use duration to avoid pesky negatives for expired bans
			return $"{ts.Days:00}:{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
		}

		/// <summary>
		/// Returns a string in the format dd:mm:hh:ss indicating the time elapsed since the ban was added.
		/// </summary>
		/// <returns></returns>
		public string GetPrettyTimeSinceBanString()
		{
			TimeSpan ts = ((DateTime)DateTime.UtcNow - TimeBanned).Duration();
			return $"{ts.Days:00}:{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
		}

	}
}
