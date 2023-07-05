using Microsoft.Xna.Framework;
using MongoDB.Entities;

namespace TShockAPI
{
	/// <summary>
	/// Represents a warp.
	/// </summary>
	public class Warp : Entity
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Gets or sets the warp's privacy state.
		/// </summary>
		public bool IsPrivate { get; set; }
		/// <summary>
		/// Gets or sets the position.
		/// </summary>
		public Point Position { get; set; }

		public string WorldID { get; set; }

		public Warp(Point position, string name, bool isPrivate = false)
		{
			Name = name;
			Position = position;
			IsPrivate = isPrivate;
		}

		/// <summary>Creates a warp with a default coordinate of zero, an empty name, public.</summary>
		public Warp()
		{
			Position = Point.Zero;
			Name = "";
			IsPrivate = false;
		}
	}
}
