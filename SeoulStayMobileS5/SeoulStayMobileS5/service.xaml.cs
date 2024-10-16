using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private decimal totalAmount; // Keeps track of the total before discount
        private decimal discountedTotal; // Keeps track of the total after applying the discount
        private ObservableCollection<UserPurchase> cartItems = new ObservableCollection<UserPurchase>();

        public TabbedPage1()
        {
            InitializeComponent();
            LoadSomeData();
            //LoadCartItems();
            LoadServiceTypes();
            this.CurrentPageChanged += OnCurrentPageChanged;
            
        }

        protected  override  void OnAppearing()
        {
            base.OnAppearing();
              LoadCartItems();
            UpdateTotalAmount();
        }

        private void OnCurrentPageChanged(object sender, EventArgs e)
        {
            // Set the TabbedPage title to the title of the current page
            if (CurrentPage == cartName)
            {
                this.Title = "Seoul Stay - Cart Checkout";

            } else if (CurrentPage == servicesTab)
            {
                this.Title = "Seoul Stay - Services Menu";
            } else
            {
                this.Title = "Seoul Stay - About Us";
            }
        }



        private async void LoadServiceTypes()
        {
            using (HttpClient client = new HttpClient())
            {
                var url = "http://10.0.2.2:8044/api/servicetypes";
                try
                {
                    var response = await client.GetStringAsync(url);
                    var serviceTypes = JsonConvert.DeserializeObject<ObservableCollection<ServiceType>>(response);

                    // Bind the ListView to the service types
                    serviceTypesList.ItemsSource = serviceTypes;
                }
                catch (HttpRequestException ex)
                {
                    await DisplayAlert("Error", $"Unable to load service types: {ex.Message}", "OK");
                }
            }
        }

        private async void ServiceType_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            var selectedServiceType = (ServiceType)e.SelectedItem;
            await DisplayAlert("Selected Service", $"You selected: {selectedServiceType.Name}", "OK");

            if (selectedServiceType.Name == "City tours")
            {
                await Navigation.PushAsync(new cityServicePage());
            }
            else if (selectedServiceType.Name == "Attraction tickets")
            {
                await Navigation.PushAsync(new attractionService());
            }
            else if (selectedServiceType.Name == "Airport Transfer")
            {
                await Navigation.PushAsync(new airportService());
            }
            else if (selectedServiceType.Name == "Catering services")
            {
                await Navigation.PushAsync(new cateringService());
            }
            else if (selectedServiceType.Name == "Safety box")
            {
                await Navigation.PushAsync(new safetyBox());
            }
        }

        public class ServiceType
        {
            public int Id { get; set; }
            public string Guid { get; set; }
            public string Name { get; set; }
            public string IconName { get; set; }
            public string Description { get; set; }
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

                var url = $"http://10.0.2.2:8044/api/userpurchases?userId={userId}";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);
                    cartItems = JsonConvert.DeserializeObject<ObservableCollection<UserPurchase>>(response);

                    cartListView.ItemsSource = cartItems;

                    UpdateTotalAmount();
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
            // Calculate total amount based on the cart items
            totalAmount = cartItems.Sum(item => item.TotalPrice);
            discountedTotal = totalAmount; // Initially, discounted total is the same as the totalAmount
            totalAmountToPay.Text = $"Total amount payable ({cartItems.Count} items): ${totalAmount:F2}";
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

            if (couponValid != null)
            {
                await DisplayAlert("Success", "You wrote a correct coupon code", "Ok");
                ApplyDiscount(couponValid); // Apply discount after checking coupon validity
            }
            else
            {
                await DisplayAlert("Error", "You wrote an incorrect coupon code", "Ok");
            }
        }

        public class Coupons
        {
            public int Id { get; set; }
            public string CouponCode { get; set; }
            public decimal DiscountPercent { get; set; }
            public decimal MaximimDiscountAmount { get; set; }
        }

        private void ApplyDiscount(Coupons coupon)
        {
            // Calculate the discount based on the coupon percent
            decimal discount = (coupon.DiscountPercent / 100) * totalAmount;

            // Ensure the discount does not exceed the maximum allowed by the coupon
            if (discount > coupon.MaximimDiscountAmount)
            {
                discount = coupon.MaximimDiscountAmount;
            }

            // Update discounted total after applying the discount
            discountedTotal = totalAmount - discount;

            // Update the total amount displayed on the UI
            totalAmountToPay.Text = $"Total amount payable ({cartItems.Count} items): ${discountedTotal:F2}";

            // Inform the user how much they saved
            DisplayAlert("Coupon Applied", $"You saved ${discount:F2}!", "Ok");

            // Disable the submit button to prevent re-applying the coupon
            successCoupon.Text = "Coupon Successfully Applied";
            submitBtn.IsEnabled = false;
        }

        private async Task<Coupons> couponIsRight(string couponCode)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "http://10.0.2.2:8044/api/coupons";
                var response = await client.GetStringAsync(apiUrl);
                var coupons = JsonConvert.DeserializeObject<List<Coupons>>(response);

                // Find the matching coupon by its code
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
                UpdateTotalAmount();
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

        private async void proceedBtn_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirm Checkout", "Do you want to finalize the purchase?", "Yes", "NO");

            if (!confirm)
                return;

            var userId = await SecureStorage.GetAsync("userId");

            if (string.IsNullOrEmpty(userId))
            {
                await DisplayAlert("Error", "User not found", "OK");
                return;
            }

            using (HttpClient client = new HttpClient())
            {
                var deleteUrl = $"http://10.0.2.2:8044/api/UserPurchases/ClearCart?userId={userId}";
                var response = await client.DeleteAsync(deleteUrl);

                if (response.IsSuccessStatusCode)
                {
                    cartItems.Clear();
                    cartListView.ItemsSource = null;
                    cartListView.ItemsSource = cartItems;
                    totalAmountToPay.Text = "Total amount payable: $0.00";

                    await DisplayAlert("Success", "Checkout completed successfully.", "OK");
                } else
                {
                    await DisplayAlert("Error", "Failed to finalize the purchase", "OK");
                }
            }
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

        
    }
}