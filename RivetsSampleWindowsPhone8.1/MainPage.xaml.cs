using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace RivetsSampleWindowsPhone8._1
{
    using System.Diagnostics;

    using Windows.Phone.UI.Input;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }


        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                backPressedEventArgs.Handled = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private void buttonNavigate_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Product), "widget");
        }

        private async void buttonAppLink_Click(object sender, RoutedEventArgs e)
        {
            var url = "https://rawgit.com/Redth/Rivets/master/Rivets.Tests/Html/SimpleWindowsPhoneMetaData.html";
            var result = await Rivets.AppLinks.Navigator.Navigate(url);

            Debug.WriteLine(result);
        }

        private async void buttonWebFallback_Click(object sender, RoutedEventArgs e)
        {
            var url = "https://rawgit.com/Redth/Rivets/master/Rivets.Tests/Html/WebFallbackMetaData.html";
            var result = await Rivets.AppLinks.Navigator.Navigate(url);

            Debug.WriteLine(result);
        }
        #endregion
    }
}
