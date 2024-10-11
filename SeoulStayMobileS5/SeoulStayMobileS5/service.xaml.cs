using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SeoulStayMobileS5
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabbedPage1 : TabbedPage
    {
        private List<CartItem> cartItems = new List<CartItem>();
        public TabbedPage1()
        {
            InitializeComponent();
            UpdateCartListView();
            UpdateCartTabTitle();
        }

        public void AddToCart(CartItem item)
        {
            cartItems.Add(item);
            UpdateCartListView();
            UpdateCartTabTitle();
        }

        private void UpdateCartTabTitle()
        {
            int itemCount = cartItems.Count;
            this.Children[1].Title = $"Cart ({itemCount})";
        }

        private void UpdateCartListView()
        {
            cartListView.ItemsSource = null;
            cartListView.ItemsSource = cartItems;
        }

        private async void cityTourBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new cityServicePage());
        }

        private void attractionBtn_Clicked(object sender, EventArgs e)
        {

        }

        private void transferBtn_Clicked(object sender, EventArgs e)
        {

        }

        private void cateringBtn_Clicked(object sender, EventArgs e)
        {

        }

        private void safetyBtn_Clicked(object sender, EventArgs e)
        {

        }

        private void deleteBtn_Clicked(object sender, EventArgs e)
        {

        }
    }
}