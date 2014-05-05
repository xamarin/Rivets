using System;
using System.Threading.Tasks;

namespace Rivets
{
	public interface IAppLinkResolver
	{
		Task<AppLink> ResolveAppLinks(string url);

		Task<AppLink> ResolveAppLinks(Uri url);
	}
}

