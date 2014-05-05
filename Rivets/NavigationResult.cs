using System;
using System.Threading.Tasks;

namespace Rivets
{
	public enum NavigationResult {
		// Indicates that the navigation failed and no app was opened.
		Failed,
		// Indicates that the navigation succeeded by opening the URL in the browser.
		Web,
		// Indicates that the navigation succeeded by opening the URL in an app on the device.
		App
	}
	
}
