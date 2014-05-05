using System;
using System.Collections.Generic;

namespace Rivets
{
	public class AppLink
	{
		public Uri SourceUrl { get; set; }
		public Uri WebUrl { get; set; }
		public List<IAppLinkTarget> Targets { get; set; }
	}	
}
