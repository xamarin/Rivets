using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using RivetsSampleWinPhone8.Resources;

namespace RivetsSampleWinPhone8
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void buttonNavigate_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Product.xaml?id=widget", UriKind.Relative));
        }

        private async void buttonAppLink_Click(object sender, RoutedEventArgs e)
        {
            var url = "https://rawgit.com/Redth/Rivets/master/Rivets.Tests/Html/SimpleWindowsPhoneMetaData.html";
            var result = await Rivets.AppLinks.Navigator.Navigate(url);

            Console.WriteLine(result);
        }

        private async void buttonWebFallback_Click(object sender, RoutedEventArgs e)
        {
            var url = "https://rawgit.com/Redth/Rivets/master/Rivets.Tests/Html/WebFallbackMetaData.html";
            var result = await Rivets.AppLinks.Navigator.Navigate(url);

            Console.WriteLine(result);
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}