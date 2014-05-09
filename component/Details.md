Rivets is a C# implementation of [App Links](http://applinks.org). It's functionally a port of Bolts, the Java/Obj-C implementation. Rivets is still in alpha and will be available on NuGet as a PCL when it is ready for its first release!

## What are App Links?

App Links are a new open source, cross-platform standard for helping link between Mobile and the Web (and vice-versa). 

App Links are a defined set of metadata that can be advertised within html of web pages that specify how to deep link to content inside of a Mobile app.  App Links are about the ***discovery*** of ways to *link between Mobile and Web*.

 - ***Mobile Deep Linking from the Web*** - Web pages can advertise special <metadata ... /> tags within a normal web page, which specify how to deep link to content inside of a particular Mobile app.
 - ***Mobile to Mobile Linking*** - Mobile apps can resolve meta data from Web links into links for other mobile apps.
How does it work?

Let's say you have a link: `http://example.com/products/widget` which displays information about a Widget you can buy on the web.

What if you also had a native Mobile app which could display this information? How would you describe how to send users to it? Most platforms have some means of 'deep-linking' inside of an app, but each platform does it a little bit differently.

Using App Links, you can resolve what different mobile platform links are available for a given url. For example, if you resolve the app links for the url mentioned above, you could find that the equivalent deep link url for the iOS app is actually: `example://products?id=widget` (as is specified by the <metadata ... /> tags right within the web page's html).


## Learn More
Learn more about App Links by visiting http://applinks.org