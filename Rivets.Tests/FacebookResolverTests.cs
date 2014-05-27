using System;
using System.Configuration;
using System.Linq;
using NUnit.Framework;

namespace Rivets.Tests
{
	public class FacebookResolverTests
	{
		public FacebookResolverTests ()
		{
		}

		[SetUp]
		public void Setup()
		{
			FB_APP_ID = ConfigurationManager.AppSettings ["FB_APP_ID"];
			FB_CLIENT_TOKEN = ConfigurationManager.AppSettings ["FB_CLIENT_TOKEN"];
		}

		string FB_APP_ID;
		string FB_CLIENT_TOKEN;

		[Test]
		public void FacebookIndexTest()
		{
			var fb = new FacebookIndexResolver(FB_APP_ID, FB_CLIENT_TOKEN);
			var al = fb.ResolveAppLinks ("http://fb.me/729250327126474").Result;

			Assert.IsNotNull (al);
			Assert.IsNotNull (al.Targets);
			Assert.Greater (al.Targets.Count, 0);
			Assert.IsNotNull (al.Targets.FirstOrDefault (t => t is IOSAppLinkTarget));
		}

		[Test]
		public void NoAppLinksFoundTest()
		{
			var fb = new FacebookIndexResolver(FB_APP_ID, FB_CLIENT_TOKEN);
			var al = fb.ResolveAppLinks ("http://some.fictional.url.com").Result;

			Assert.IsNotNull (al);
			Assert.IsNotNull (al.Targets);
			Assert.AreEqual (al.Targets.Count, 0);
		}
	}
}

