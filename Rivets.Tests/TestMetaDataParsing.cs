using System;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using NUnit.Framework;
using Owin;

namespace Rivets.Tests
{
	[TestFixture ]
	public class TestMetaDataParsing
	{
		const string HOST_BASE = "http://localhost:4477/";

		[SetUp]
		public void Setup()
		{
			var root = System.IO.Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Html");
			var options = new FileServerOptions
			{
				EnableDirectoryBrowsing = true, 
				FileSystem = new PhysicalFileSystem(root)
			};

			WebApp.Start(HOST_BASE, builder => builder.UseFileServer(options));            
		}

		[Test]
		public void SimpleAndroidMetaDataTest ()
		{
			var resolver = new HttpClientAppLinkResolver ();
			var appLinks = resolver.ResolveAppLinks (HOST_BASE + "SimpleAndroidMetaData.html").Result;

			Assert.IsNotNull (appLinks);
			Assert.Greater (appLinks.Targets.Count, 0);
		}
	}
}

