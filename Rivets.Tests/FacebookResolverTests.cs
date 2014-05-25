using System;
using System.Configuration;
using NUnit.Framework;

namespace Rivets.Tests
{
	public class FacebookResolverTests
	{
		public FacebookResolverTests ()
		{
		}

		[SetUpFixture]
		void Setup()
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


		}
	}
}

