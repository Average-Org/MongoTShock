using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity = MongoDB.Entities.Entity;

namespace TShockAPI.Models.Entities
{
	public class LastPosition : Entity
	{
		public string Name { get; set; }
		public string IP { get; set; }

		public Vector2 Position => new(X, Y);
		public int X { get; set; }
		public int Y { get; set; }
		public string WorldID { get; set; }
	}
}
