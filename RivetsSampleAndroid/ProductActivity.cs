
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
	[IntentFilter(new [] {Android.Content.Intent.ActionView }, DataScheme="example", DataHost="*", Categories=new [] { Android.Content.Intent.CategoryDefault })]
	public class ProductActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			var id = string.Empty;

			if (Intent.HasExtra ("PRODUCT_ID")) {
				id = Intent.GetStringExtra ("PRODUCT_ID");
			} else {
				var alUrl = new Rivets.AppLinkUrl (Intent.Data.ToString ());

				if (alUrl != null && alUrl.InputQueryParameters.ContainsKey ("id")) {
					id = alUrl.InputQueryParameters ["id"];
				}
			}

			Toast.MakeText (this, "Display Product Id: " + id, ToastLength.Short).Show ();
		}
	}
}

