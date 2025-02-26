using NUnit.Framework;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;

namespace TShockLauncher.Tests;

public class GroupTests
{
	/// <summary>
	/// This tests to ensure the group commands work.
	/// </summary>
	/// <remarks>Due to the switch to Microsoft.Data.Sqlite, nulls have to be replaced with DBNull for the query to complete</remarks>
	[TestCase]
	public void TestPermissions()
	{
		var groups = TShock.Groups = new GroupManager();

		if (!groups.GroupExists("test"))
			groups.AddGroup("test", null, new string[] { "test" }, Group.defaultChatColor);

		groups.AddPermissions("test", new() { "abc" });

		var hasperm = groups.GetGroupByName("test").Permissions.ToList().Contains("abc");
		Assert.IsTrue(hasperm);

		groups.DeletePermissions("test", new() { "abc" });

		hasperm = groups.GetGroupByName("test").Permissions.ToList().Contains("abc");
		Assert.IsFalse(hasperm);

		groups.DeleteGroup("test");

		var g = groups.GetGroupByName("test");
		Assert.IsNull(g);
	}
}

