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

namespace SeoulStayMobileS5
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class cityServicePage : ContentPage
	{
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

            var selectedItem = (Service)e.SelectedItem;

            titleOfService.Text = $"Description of {selectedItem.name}";
            descriptionOfService.Text = selectedItem.description;
        }

        private void date_DateSelected(object sender, DateChangedEventArgs e)
        {
            var selectedDate = e.NewDate;

            if(isValidDate(selectedDate, selectedService))
            {
                spotsLabel.Text = $"Remaining: {CalculateRemainingSpots(selectedDate, selectedService)}";
            }
            else
            {
                DisplayAlert("Invalid Date", "The selected date is not available for booking.", "OK");
            }
        }

       /* private bool isValidDate(DateTime selectedDate, Service selectedService)
        {
            bool validDayOfWeek = true;
            if (!string.IsNullOrEmpty(selectedService.dayOfWeek) && selectedService.dayOfWeek != "*")
            {
                var allowedDaysOfWeek = selectedService.dayOfWeek.Split('-').Select(int.Parse).ToArray();
                int selectedDayOfWeek = (int)selectedDate.DayOfWeek;

                validDayOfWeek = selectedDayOfWeek >= allowedDaysOfWeek[0] && selectedDayOfWeek <= allowedDaysOfWeek[1];

            }

            bool validDayOfMonth = true;
            if(!string.IsNullOrEmpty(selectedService.dayOfMonth) && selectedService.dayOfMonth != "*")
            {
                var allowedDaysOfMonth = selectedService.dayOfMonth.Split(',')
                    .SelectMany(day => ExpandRange(day)).ToList();

                int selectedDaysOfMonth = selectedDate.Day;

                validDayOfMonth = allowedDaysOfMonth.Contains(selectedDaysOfMonth);
            }

            return validDayOfWeek && validDayOfMonth;
        }

        private IEnumerable<int> ExpandRange(string range)
        {
            if (range.Contains('-'))
            {
                var parts = range.Split('-').Select(int.Parse).ToArray();
                return Enumerable.Range(parts[0], parts[1] - parts[0] + 1);
            }
            return new List<int> { int.Parse(range) };
        }

        private int CalculateRemainingSpots(DateTime selectedDate, Service service)
        {
            // Fetch how many bookings are already made for this date (from your backend or cache)
            int bookingsMade = GetBookingsForDate(selectedDate);

            // Remaining spots = Daily cap - Bookings made
            return service.dailyCap - bookingsMade;
        }
*/

        private void addBtn_Clicked(object sender, EventArgs e)
        {

        }

        private void numOfPeople_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }



	
}