using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace RivetsSampleWinPhone8
{
    class AppLinkUriMapper : UriMapperBase
    {
        string tempUri;

        const string EXT_LAUNCH_URI = "/Protocol?encodedLaunchUri=";

        public override Uri MapUri(Uri uri)
        {
            var url = uri.ToString();

            // If it's not a special case where we were launched from externally, just return the uri with no mapping changes
            if (!url.StartsWith(EXT_LAUNCH_URI))
                return uri;

            // Get the encodedLaunchUri Query parameter which is actually the url we are interested in
            var launchUrl = System.Net.HttpUtility.UrlDecode(url.Substring(url.IndexOf(EXT_LAUNCH_URI) + EXT_LAUNCH_URI.Length));
            
            // Build the AppLinkUrl from this url passed in
            var apUrl = new Rivets.AppLinkUrl(launchUrl);

            // See if the url fits some deep linking rules for our app
            if (apUrl.InputUrl.Scheme.StartsWith("example") && apUrl.InputUrl.Host.Contains("product"))
            {
                // Get the id parameter
                var id = string.Empty;
                if (apUrl.InputQueryParameters.ContainsKey("id"))
                    id = apUrl.InputQueryParameters["id"];

                // Finally, navigate to the product page (deep link)
                return new Uri("/Product.xaml?id=" + id, UriKind.Relative);
            }

            return uri;                        
        }
    }
}
