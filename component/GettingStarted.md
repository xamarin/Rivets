# Getting Started with Rivets
It's important to understand what App Links actually are first.  You can visit the [App Links](http://applinks.org) as well as read the [official documentation](http://applinks.org/documentation/).  There's also a good [blog post](http://redth.info/what-are-app-links) helping explain App Links.

## Replacing your Navigation with App Link Navigation
One of the most important parts of making App Link adoption a success is to make your own apps use App Link navigation in place of traditional web URL navigation.  

You should replace any navigation calls like this:

```
// iOS
UIApplication.SharedApplication.OpenUrl("http://example.com/product/1");

// Android
StartActivity(new Intent(Intent.ActionView, Uri.Parse("http://example.com/product/1"));

// Windows Phone
Windows.System.Launcher.LaunchUriAsync(new Uri("http://example.com/product/1"));
```
With App Link navigation using Rivets like this:

```
var result = await Rivets.AppLinks.Navigator.Navigate("http://example.com/product/1");
``` 

Every URL you navigate to using Rivets will first be checked for App Link metadata, and an attempt to Navigate to a native app will be made.  If this is not possible, an attempt to navigate to a fallback web URL will be made.  Finally, if that also fails, the `NavigationResult` returned by `Navigate(..)` will be `Failed` and you can handle the situation how you see fit (eg: you may want to still navigate to the original web URL).

## Handling Incoming App Link Navigation

If you want other apps to be able to App Link to your native apps there are a few steps to follow.


#### Add App Link MetaData to your Web Pages

First of all, your web page content should include the special `<metadata .. />` tags defining what different platforms you have apps on, and what the deep link to those apps are for the given web page's content.

You can read more about this in the [official documentation](http://applinks.org/documentation/), but here is an example of what you might add to your web page:

```html
<head>
 <!-- iOS -->
 <meta property="al:ios:url" content="example://products?id=1" />
 <meta property="al:ios:app_store_id" content="12345" />
 <meta property="al:ios:app_name" content="Example App" />
 
 <!-- Android -->
 <meta property="al:android:url" content="example://products?id=1" />
 <meta property="al:android:package" content="com.example.app" />
 <meta property="al:android:app_name" content="Example App" />
 
 <!-- Windows Phone -->
 <meta property="al:windows_phone:url" content="example://products?id=1" />
 <meta property="al:windows_phone:app_id" content="abcde-guid-12345" />
 <meta property="al:windows_phone:app_name" content="Example App" />
  
 <!-- Web Fallback -->
 <meta property="al:web:url" content="http://example.com/product/1" />
</head>
```


#### Registering your App for handling URL's

Most platforms have a concept of being able to receive incoming information from other apps.  iOS and Windows Phone platforms require that you register your app to handle a specific URL scheme.  Android is a bit different in that you can describe what types of Intents your various activities can be selected to handle.  This is nothing specific to Rivets or App Links.

To register your app to handle a URL scheme on iOS, you would need to add the following to your `Info.plist` file:

![Info.plist Example](https://raw.githubusercontent.com/Redth/Rivets/master/component/iOSInfoPlist.png)

To register your app to handle a URL scheme on Windows phone, you would add the following to your `WPManifest.xml` file (after the closing Tokens tag, and before the opening ScreenResolutions tag):

```xml
</Tokens>
<Extensions>
  <Protocol Name="example" NavUriFragment="encodedLaunchUri=%s" TaskID="_default" />
</Extensions>
<ScreenResolutions>
```

On Android, you need to mark up your Intent with a Filter for your URL scheme.  You could do this by editing the `AndroidManifest.xml` file, or you can add some attributes to your activity like this:

```
[Activity (Label = "Product Details")]			
[IntentFilter(new [] {Android.Content.Intent.ActionView }, 
	DataScheme="example", 
	DataHost="*", 
	Categories=new [] { Android.Content.Intent.CategoryDefault })]
public class ProductActivity : Activity
{
	// ...
}
```


#### Parsing Incoming App Link data

There's a special class called `AppLinkUrl` to help you parse the data you receive from incoming App Links.

On iOS in your AppDelegate, OpenUrl should be overridden to handle incoming links.  You can construct and use an `AppLinkUrl` instance something like this:

```csharp
public override bool OpenUrl (UIApplication app, NSUrl url, string srcApp, NSObject annotation)
{
	var rurl = new Rivets.AppLinkUrl (url.ToString ());

	if (rurl.InputUrl.Host.Equals ("products")) {
		var id = rurl.InputQueryParameters ["id"];

		var c = new ProductViewController (id, rurl.Referrer);
		navController.PushViewController (c, true);
		return true;
	}
	
	return false;
}

```

On Android since your Activity registers the Intent filter, it will be directly opened for you.  You will still need to parse the `al_applink_data` intent which contains the App Link metadata used to launch the app:

```
protected override void OnCreate (Bundle bundle)
{
	base.OnCreate (bundle);

	SetContentView (Resource.Layout.ProductLayout);

	var id = string.Empty;

	// See if the activity was opened from an internal Intent
	// which could have the product id in the Extras
	if (Intent.HasExtra ("PRODUCT_ID")) {
		id = Intent.GetStringExtra ("PRODUCT_ID");
	} else {

		var appLinkData = Intent.GetStringExtra ("al_applink_data");

		// Otherwise, check and see if we were launched from an AppLink
		// and if so, Parse the url from the Intent Data
		var alUrl = new Rivets.AppLinkUrl (Intent.Data.ToString (), appLinkData);

		// TargetQueryParameters will contain our product id
		if (alUrl != null && alUrl.InputQueryParameters.ContainsKey ("id")) {
			id = alUrl.TargetQueryParameters ["id"];
		}
	}
}
```

On Windows Phone you need to subclass UriMapper:

```
class MyAppLinkUriMapper : UriMapperBase
{
    const string EXT_LAUNCH_URI = "/Protocol?encodedLaunchUri=";

    public override Uri MapUri(Uri uri)
    {
        var url = uri.ToString();

        if (!url.StartsWith(EXT_LAUNCH_URI))
            return uri;

        // Get the encodedLaunchUri Query parameter 
        // which is actually the url we are interested in
        var launchUrl = HttpUtility.UrlDecode(url.Substring(url.IndexOf(EXT_LAUNCH_URI) + EXT_LAUNCH_URI.Length));
        
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
```

NOTE: You'll need to set your `RootFrame.UriMapper = new MyAppLinkUriMapper();` in your App's initialization.


## Using Facebook's Index to Resolve AppLinks
To help with the adoption and lower the overhead required to parse AppLinks, Facebook has created their own public index.  Instead of you fetching the HTML content of a page and parsing the AppLink metadata yourself (on a mobile device), you can query the Facebook Index instead. 

When you send a request to the Facebook Index, it will first check its cache to see if it has been asked about this content before, and if so, it will quickly returned cached results to you.  If it has nothing in its cache, it will go out and parse the HTML for you and return any results it finds.

There are a couple advantages to using Facebook Index to resolve AppLinks:

1. Speed: Cached results are returned VERY quickly, and even if there are no cached results, Facebook has much better peering to data centers around the world than your user's mobile device, plus downloading and parsing HTML on the server side is going to be a lot quicker than a mobile device.
2. Lower bandwidth: Facebook returns its results as JSON and therefore the bandwidth usage is much lower than downloading entire HTML pages and parsing them on a mobile device.

You can use the Facebook resolver simply by setting it to be the default app link resolver:

```csharp
AppLinks.DefaultResolver = new FacebookIndexAppLinkResolver ("YOUR-FB-APP-ID", "YOUR-FB-APP-TOKEN");
```

NOTE: To use the Facebook Index you must provide an **App ID** and **App Client Token**.  You can get these by signing up at the Facebook Developer's site, and creating an Application.  The App ID will be listed on your Facebook Application's Dashboard.  The Client Token comes from the Settings -> Advanced page, under Security (Client Token).


