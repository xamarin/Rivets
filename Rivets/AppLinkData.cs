using System;
using System.Runtime.Serialization;

namespace Rivets
{
	[DataContract]
	public class AppLinkData
	{
		public AppLinkData ()
		{
			Version = "1.0";
			UserAgent = "Rivets.NET 1.0";
		}

		[DataMember(Name="target_url")]
		public string TargetUrl { get;set; }

		[DataMember(Name="version")]
		public string Version { get;set; }

		[DataMember(Name="user_agent")]
		public string UserAgent { get;set; }

		[DataMember(Name="extras")]
		public string Extras { get;set; }
	}
}

