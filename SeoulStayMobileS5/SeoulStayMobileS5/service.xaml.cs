using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static SeoulStayMobileS5.MainPage;
using static SeoulStayMobileS5.TabbedPage1;

namespace SeoulStayMobileS5
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabbedPage1 : TabbedPage
    {

        private List<UserPurchase> cartItems = new List<UserPurchase>();
        private decimal totalAmount;

        public TabbedPage1()
        {
            InitializeComponent();
            LoadSomeData();
            LoadCartItems();

        }

        private async void LoadSomeData()
        {
            try
            {
                var fullName = await SecureStorage.GetAsync("userFullname");
                welcomMessage.Text = $"Welcome {fullName}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"{ex}", "Ok");
            }

        }

        private async void LoadCartItems()
        {

            try
            {
                var userId = await SecureStorage.GetAsync("userId");
                if (string.IsNullOrEmpty(userId))
                {
                    await DisplayAlert("Error", "User not found", "Ok");
                    return;
                }

                string apiUrl = "http://10.0.2.2:8044/api/userpurchases?userId={userId}";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(apiUrl);
                    cartItems = JsonConvert.DeserializeObject<List<UserPurchase>>(response);

                    cartListView.ItemsSource = cartItems;

                    cartName.Title = $"Cart ({cartItems.Count})";
                }
            }
            catch (Exception ex) 
            {
                await DisplayAlert("Error", $"{ex}", "Ok");

            }
        }

        private void UpdateTotalAmount()
        {
            decimal total = cartItems.Sum(item => item.TotalPrice);
            totalAmountToPay.Text = $"${total:F2}";

        }

        private async void cityTourBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new cityServicePage());
        }

        private async void attractionBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new cityServicePage());
        }

        private async void transferBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new cityServicePage());
        }

        private async void cateringBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new cityServicePage());
        }

        private async void safetyBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new cityServicePage());
        }

        private async void submitBtn_Clicked(object sender, EventArgs e)
        {
            string couponCode = couponField.Text;

            if (string.IsNullOrEmpty(couponCode))
            {
                await DisplayAlert("Error", "Please enter a coupon code.", "Ok");
                return;
            }

            var couponValid = await couponIsRight(couponCode);

            if(couponValid != null)
            {
                await DisplayAlert("Success", "You wrote correct coupon code", "Ok");
            } else
            {
                await DisplayAlert("Error", "You wrote incorrect coupon code", "Ok");
            }
        }

        private async Task<Coupons> couponIsRight(string couponCode)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "http://10.0.2.2:8044/api/coupons";
                var response = await client.GetStringAsync(apiUrl);
                var coupons = JsonConvert.DeserializeObject<List<Coupons>>(response);

                var coupon = coupons.FirstOrDefault(u => u.CouponCode == couponCode);
                return coupon;
            }
        }

        private async void deleteBtn_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var userPurchase = button?.CommandParameter as UserPurchase;


            if (userPurchase == null)
            {
                await DisplayAlert("Error", "Unable to find the item to delete.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Delete", "Are you sure you want to delete this item?", "Yes", "No");

            if (!confirm)
                return;

            var isSuccess = await DeleteUserPurchaseAsync(userPurchase.Id);

            if (isSuccess)
            {
                await DisplayAlert("Success", "Item deleted successfully.", "OK");

                // Remove the item from the list and refresh the cart
                cartItems.Remove(userPurchase);
                cartListView.ItemsSource = null;
                cartListView.ItemsSource = cartItems;
            }
            else
            {
                await DisplayAlert("Error", "There was an issue deleting the item. Please try again.", "OK");
            }

        }

        private async Task<bool> DeleteUserPurchaseAsync(long id)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"http://10.0.2.2:8044/api/userpurchases/{id}";
                var response = await client.DeleteAsync(apiUrl);

                return response.IsSuccessStatusCode;
            }
        }

        private void proceedBtn_Clicked(object sender, EventArgs e)
        {

        }

        public class UserPurchase
        {
            public long Id { get; set; }
            public long UserId { get; set; }
            public string Service { get; set; }
            public decimal TotalPrice { get; set; }
            public DateTime Date { get; set; }
            public string UserNotes { get; set; }
            public long NumberOfPeople { get; set; }
            public string Refunded { get; set; }
        }

        public class Coupons
        {
            public int Id { get; set; }
            public string CouponCode { get; set; }
            public decimal discountPercent { get; set; }
            public decimal maximimDiscountAmount { get; set; }
        }
    }
}