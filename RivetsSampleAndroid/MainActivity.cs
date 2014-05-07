using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;

namespace RivetsSampleAndroid
{
	[Activity (Label = "Rivets Sample", MainLauncher = true)]
	public class MainActivity : ListActivity
	{
		MainListAdapter adapter;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			//SetContentView (Resource.Layout.Main);

			adapter = new MainListAdapter {
				Context = this
			};

			ListAdapter = adapter;

			ListView.ItemClick += async (sender, e) => {
				var item = adapter[e.Position];

				if (item == "Product Details: widget") {

					var intent = new Intent(this, typeof(ProductActivity));
					intent.PutExtra("PRODUCT_ID", "widget");
					StartActivity(intent);

				} else if (item == "App Link to widget") {

					var url = "https://rawgit.com/Redth/Rivets/master/Rivets.Tests/Html/SimpleAndroidMetaData.html";
					var result = await Rivets.AppLinks.Navigator.Navigate(url);

					Console.WriteLine(result);
				} else {

					var url = "https://rawgit.com/Redth/Rivets/master/Rivets.Tests/Html/WebFallbackMetaData.html";
					var result = await Rivets.AppLinks.Navigator.Navigate(url);

					Console.WriteLine(result);
				}
			};
		}
	}


	public class MainListAdapter : BaseAdapter<string>
	{
		public Activity Context { get;set; }

		List<string> items = new List<string> {
			"Product Details: widget",
			"App Link to widget",
			"Web Fallback Link"
		};

		public override long GetItemId (int position) { return position; }
		public override int Count { get { return items.Count; } }
		public override string this [int index] { get { return items[index]; } }

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var item = items [position];

			var view = convertView ?? Context.LayoutInflater.Inflate (Android.Resource.Layout.SimpleListItem1, null);

			var text1 = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			text1.Text = item;

			return view;
		}
	}
}


