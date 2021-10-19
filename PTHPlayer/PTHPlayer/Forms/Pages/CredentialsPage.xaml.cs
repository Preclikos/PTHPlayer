using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Models;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CredentialsPage : ContentPage
    {
        DataService DataStorageService;
        HTSPController HTSPConnectionController;

        public CredentialsPage(DataService dataService, HTSPController hTSPConnectionController, bool invalidLogin = false)
        {
            InitializeComponent();
            DataStorageService = dataService;
            HTSPConnectionController = hTSPConnectionController;
            if(invalidLogin)
            {
                Status.Text = "Invalid Login Credentials!";
                Status.TextColor = Color.Red;
                return;
            }
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {

            if(string.IsNullOrEmpty(Server.Text) || string.IsNullOrEmpty(Port.Text) || string.IsNullOrEmpty(UserName.Text) || string.IsNullOrEmpty(Password.Text))
            {
                Status.Text = "Please fill all fields!";
                Status.TextColor = Color.Red;
                return;
            }

            int port = 0;
            if(!int.TryParse(Port.Text, out port))
            {
                Status.Text = "Wrong port inserted!";
                Status.TextColor = Color.Red;
                return;
            }

            var credentials = new CredentialsModel
            {
                Server = Server.Text,
                Port = port,
                UserName = UserName.Text,
                Password = Password.Text
            };
            DataStorageService.SetCredentials(credentials);
            try
            {
                HTSPConnectionController.Connect();
                Navigation.RemovePage(this);
            }
            catch (Exception ex)
            {
                DataStorageService.ClearCredentials();
                Status.Text = ex.Message;
                Status.TextColor = Color.Red;
            }
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }


        private void Entry_Focused(object sender, FocusEventArgs e)
        {
            var senderEntry = (Entry)sender;
            ((StackLayout)senderEntry.Parent).BackgroundColor = Color.DarkCyan;
        }

        private void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            var senderEntry = (Entry)sender;
            ((StackLayout)senderEntry.Parent).BackgroundColor = Color.Transparent;
        }
    }
}