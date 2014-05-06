using System;
using System.Collections.Generic;

namespace Rivets
{
	public class AppLink
	{
		/// <summary>
		/// Url that the App Link originates from
		/// </summary>
		/// <value>The source URL.</value>
		public Uri SourceUrl { get; set; }

		/// <summary>
		/// Web Url that the App Link should fall back to
		/// </summary>
		/// <value>The web URL.</value>
		public Uri WebUrl { get; set; }

		/// <summary>
		/// Mobile app Targets for the given Source Url
		/// </summary>
		/// <value>The targets.</value>
		public List<IAppLinkTarget> Targets { get; set; }
	}	
}
