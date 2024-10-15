using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SeoulStayMobileS5
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Title = "Seoul Stay Addon Services";
        }


        private async void loginBtn_Clicked(object sender, EventArgs e)
        {
            string username = usernameField.Text;
            string password = passwordField.Text;

            var user = await AuthenticateUser(username, password);

            if (user != null)
            {
                await SecureStorage.SetAsync("userId", user.Id.ToString());
                await SecureStorage.SetAsync("userFullname", user.FullName.ToString());

                await Navigation.PushAsync(new TabbedPage1());

            } else
            {
                await DisplayAlert("Error", "User or Password is incorrect", "OK");
            }

        }

        private async Task<User> AuthenticateUser(string username,string password)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "http://10.0.2.2:8044/api/Users";
                var response = await client.GetStringAsync(apiUrl);
                var users = JsonConvert.DeserializeObject<List<User>>(response);

                var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);
                return user;
            }
        }

        public class User
        {
            public int Id { get; set; }
            public string Guid { get; set; }
            public int UserTypeId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string FullName { get; set; }
            public bool Gender { get; set; }
            public DateTime BirthDate { get; set; }
            public int FamilyCount { get; set; }
        }
    }
}
