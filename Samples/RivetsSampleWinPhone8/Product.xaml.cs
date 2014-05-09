using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace RivetsSampleWinPhone8
{
    public partial class Product : PhoneApplicationPage
    {
        public Product()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Display the product ID from the querystring
            if (NavigationContext.QueryString.ContainsKey("id"))
                textProductId.Text = NavigationContext.QueryString["id"];
        }
    }
}