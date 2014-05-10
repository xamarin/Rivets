
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace RivetsSampleAndroid
{
	[Activity (Label = "ProductActivity")]			
	[IntentFilter(new [] {Android.Content.Intent.ActionView }, 
		DataScheme="example", 
		DataHost="*", 
		Categories=new [] { Android.Content.Intent.CategoryDefault })]
	public class ProductActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.ProductLayout);

			var id = string.Empty;

			// See if the activity was opened from an internal Intent
			// which would have the product id in the Extras
			if (Intent.HasExtra ("PRODUCT_ID")) {
				id = Intent.GetStringExtra ("PRODUCT_ID");
			} else {

				var appLinkData = Intent.GetStringExtra ("al_applink_data");

				// Otherwise, check and see if we were launched from an AppLink
				// and if so, Parse the url from the Intent Data
				var alUrl = new Rivets.AppLinkUrl (Intent.Data.ToString (), appLinkData);

				// InputQueryParameters will contain our product id
				if (alUrl != null && alUrl.InputQueryParameters.ContainsKey ("id")) {
					id = alUrl.InputQueryParameters ["id"];
				}
			}

			FindViewById<TextView> (Resource.Id.textViewProductId).Text = id;
		}
	}
}