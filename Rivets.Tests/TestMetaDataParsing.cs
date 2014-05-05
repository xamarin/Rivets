using NUnit.Framework;
using System;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;

namespace Rivets.Tests
{
	[TestFixture ()]
	public class TestMetaDataParsing
	{

		[SetUp]
		public void Setup()
		{
			var url = "http://localhost:4477";
			var root = System.IO.Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Html");

			var fileSystem = new PhysicalFileSystem(root);

			var options = new FileServerOptions
			{
				EnableDirectoryBrowsing = true, 
				FileSystem = fileSystem                             
			};

			WebApp.Start(url, builder => builder.UseFileServer(options));            
			Console.WriteLine("Listening at " + url);

		}

		[TearDown]
		public void TearDown()
		{

		}

		[Test ()]
		public void SimpleAndroidMetaDataTest ()
		{
			var resolver = new HttpClientAppLinkResolver ();
			var appLinks = resolver.ResolveAppLinks ("http://localhost:4477/SimpleAndroidMetaData.html").Result;

			Assert.IsNotNull (appLinks);
			Assert.Greater (appLinks.Targets.Count, 0);
		}
	}
}

