using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SeoulStayMobileS5
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class cityServicePage : ContentPage
    {
        private Service selectedService;

        public cityServicePage()
        {
            InitializeComponent();
            LoadServices();
        }

        private async void LoadServices()
        {
            using (HttpClient client = new HttpClient())
            {
                var url = "http://10.0.2.2:8044/api/services";
                try
                {
                    var response = await client.GetStringAsync(url);
                    var services = JsonConvert.DeserializeObject<List<Service>>(response);

                    var cityTours = services.Where(a => a.serviceTypeid == 1).ToList();
                    cityService.ItemsSource = cityTours;
                }
                catch (HttpRequestException ex)
                {
                    await DisplayAlert("Error", $"Error: {ex.Message}", "Ok");
                }
            }
        }

        private void cityService_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            selectedService = (Service)e.SelectedItem;
            titleOfService.Text = $"Description of {selectedService.name}";
            descriptionOfService.Text = selectedService.description;
            int numberOfPeople = int.Parse(numOfPeople.Text);
            int numberOfBooking = (int)Math.Ceiling((double)numberOfPeople / selectedService.bookingCap);
            totalAmountPay.Text = $"Amount payable: {numberOfBooking * selectedService.price:C}";
            bookingsLabel.Text = $"{numberOfBooking} bookings required";
        }

        private async void addBtn_Clicked(object sender, EventArgs e)
        {
            if (selectedService == null)
            {
                await DisplayAlert("Error", "Please select a service before adding to the cart.", "OK");
                return;
            }

            if (!int.TryParse(numOfPeople.Text, out int numberOfPeople) || numberOfPeople < 1)
            {
                await DisplayAlert("Error", "Please enter a valid number of people.", "OK");
                return;
            }

            var selectedDate = date.Date.ToString("yyyy-MM-dd");

            var userId = await SecureStorage.GetAsync("userId");

            var purchase = new UserPurchase
            {
                UserId = long.Parse(userId),  // Replace with actual user ID
                Service = selectedService.name,  // Service name
                TotalPrice = selectedService.price * numberOfPeople,  // Total price
                Date = DateTime.Parse(selectedDate),  // Correct date format
                UserNotes = addNotes.Text,  // Additional notes
                NumberOfPeople = numberOfPeople,  // Number of people
                Refunded = "NO"  // Default value
            };

            var json = JsonConvert.SerializeObject(purchase);

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Debugging: Show the serialized JSON for troubleshooting
                await DisplayAlert("Debug", json, "OK");

                try
                {
                    var response = await client.PostAsync("http://10.0.2.2:8044/api/userpurchases", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "Service added to cart successfully.", "OK");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        await DisplayAlert("Error", $"Server response: {errorResponse}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Connection error: {ex.Message}", "OK");
                }
            }
        }

        public class UserPurchase
        {
            public long UserId { get; set; }
            public string Service { get; set; } = string.Empty;
            public decimal TotalPrice { get; set; }
            public DateTime Date { get; set; }
            public string UserNotes { get; set; } = string.Empty;
            public long NumberOfPeople { get; set; }
            public string Refunded { get; set; } = "NO";
        }

        public class Service
        {
            public int id { get; set; }
            public string guid { get; set; }
            public int serviceTypeid { get; set; }
            public string name { get; set; }
            public decimal price { get; set; }
            public int duration { get; set; }
            public string description { get; set; }
            public string dayOfWeek { get; set; }
            public string dayOfMonth { get; set; }
            public int dailyCap { get; set; }
            public int bookingCap { get; set; }
        }
    }
}
