using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static SeoulStayMobileS5.cityServicePage;

namespace SeoulStayMobileS5
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class cityServicePage : ContentPage
	{
        private Service selectedService;
		public cityServicePage ()
		{
			InitializeComponent ();
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
                    await DisplayAlert("Error",$"Error: {ex}","Ok");
                    
                }
            }
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

        private void cityService_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

             selectedService = (Service)e.SelectedItem;

            titleOfService.Text = $"Description of {selectedService.name}";
            descriptionOfService.Text = selectedService.description;
            int numberOfPeople = int.Parse(numOfPeople.Text);
            int numberOfBooking = (int)Math.Ceiling((double)numberOfPeople / selectedService.bookingCap);
            totalAmountPay.Text = $"Amount payable: {numberOfBooking * selectedService.price:C}";
            bookingsLabel.Text = $"{numberOfBooking} bookings required";
        }


        private void numOfPeople_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedService == null)
                return;

            if (int.TryParse(numOfPeople.Text, out int numberOfPeople))
            {
                if (numberOfPeople < 1)
                {
                    DisplayAlert("Error", "The number of people must be at least 1.", "OK");
                    return;
                }

                int numberOfBooking = (int)Math.Ceiling((double)numberOfPeople / selectedService.bookingCap);

                totalAmountPay.Text = $"Amount payable: {numberOfBooking * selectedService.price:C}";
                bookingsLabel.Text = $"{numberOfBooking} bookings required";
            }
            else
            {
                DisplayAlert("Error", "Please enter a valid number of people.", "OK");
            }
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

            var selectedDate = date.Date;

            // Create the UserPurchase object with valid data
            var purchaseWrapper = new
            {
                userPurchase = new UserPurchase
                {
                    UserId = 1,  // Assuming UserId is 1, replace with actual user ID in a real app
                    Service = selectedService.name, // Ensure this is not null
                    TotalPrice = selectedService.price * numberOfPeople, // Calculate total price
                    Date = selectedDate, // Date should be passed in a valid format
                    UserNotes = addNotes.Text, // Notes from the user input
                    NumberOfPeople = numberOfPeople, // Number of people from user input
                    Refunded = "NO"  // Defaulting the refunded status to "NO"
                }
            };

            // Convert the object to JSON
            var json = JsonConvert.SerializeObject(purchaseWrapper);

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // For debugging, show the serialized JSON in an alert to check if the data is correct
                await DisplayAlert("Debug", json, "OK");

                try
                {
                    var response = await client.PostAsync("http://10.0.2.2:8044/api/userpurchases", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "Service added to cart successfully.", "OK");

                        // Go back to the previous page after success
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        // If there's an error, show the server's response
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        await DisplayAlert("Error", $"There was some problem. Server response: {errorResponse}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., network issues)
                    await DisplayAlert("Error", $"There was an error connecting to the server: {ex.Message}", "OK");
                }
            }
        }


    }

    public class UserPurchase
    {
        public long UserId { get; set; }
        public string Service { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime Date { get; set; }
        public string UserNotes { get; set; }
        public long NumberOfPeople { get; set; }
        public string Refunded { get; set; }
    }
}