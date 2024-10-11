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

                // Calculate the number of bookings required
                int numberOfBooking = (int)Math.Ceiling((double)numberOfPeople / selectedService.bookingCap);

                // Update the amount payable and number of bookings required
                totalAmountPay.Text = $"Amount payable: {numberOfBooking * selectedService.price:C}";
                bookingsLabel.Text = $"{numberOfBooking} bookings required";
            }
            else
            {
                DisplayAlert("Error", "Please enter a valid number of people.", "OK");
            }
        }

        private List<CartItem> cartItems = new List<CartItem>();

        private async void addBtn_Clicked(object sender, EventArgs e)
        {
            if (selectedService == null)
                return;

            if (!int.TryParse(numOfPeople.Text, out int numberOfPeople) || numberOfPeople < 1)
            {
                DisplayAlert("Error", "Please enter a valid number of people.", "OK");
                return;
            }

            var selectedDate = date.Date;
            if (selectedDate == null)
            {
                DisplayAlert("Error", "Please choose date of service", "OK");
                return;
            }

            var cartItem = new CartItem
            {
                ServiceName = selectedService.name,
                Date = date.Date,
                ServicePrice = selectedService.price,
                NumberOfPeople = numberOfPeople,
                AdditionalNotes = addNotes.Text,
                TotalAmount = numberOfPeople * selectedService.price
            };

            if (this.Parent is TabbedPage1 parentTabbedPage)
            {
                parentTabbedPage.AddToCart(cartItem);
            }

            DisplayAlert("Success", "Service added to Cart successfully", "OK");

            await Navigation.PopAsync();

        }

        //I need to change date
        private void date_DateSelected(object sender, DateChangedEventArgs e)
        {
            
        }
        

    }

    public class CartItem
    {
        public string ServiceName { get; set; }
        public DateTime Date { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal ServicePrice { get; set; }
        public string AdditionalNotes { get; set; }
        public decimal TotalAmount { get; set; }
    }
}