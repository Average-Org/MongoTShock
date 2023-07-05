using MongoDB.Entities;
using System;

namespace TShockAPI
{
	public class Research : Entity
	{
		public int WorldId { get; set; }
		public int PlayerId { get; set; }
		public int ItemId { get; set; }
		public int AmountSacrificed { get; set; }
		public DateTime TimeSacrificed { get; set; }
	}
}
