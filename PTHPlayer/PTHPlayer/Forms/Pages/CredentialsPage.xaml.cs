using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Interfaces;
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
            if (invalidLogin)
            {
                Status.Text = "Invalid Login Credentials!";
                Status.TextColor = Color.Red;
                return;
            }
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {

            ConfirmButton.IsEnabled = false;

            if (string.IsNullOrEmpty(Server.Text) || string.IsNullOrEmpty(Port.Text) || string.IsNullOrEmpty(UserName.Text) || string.IsNullOrEmpty(Password.Text))
            {
                Status.Text = "Please fill all fields!";
                Status.TextColor = Color.Red;
                ConfirmButton.IsEnabled = true;
                return;
            }

            int port = 0;
            if (!int.TryParse(Port.Text, out port))
            {
                Status.Text = "Wrong port inserted!";
                Status.TextColor = Color.Red;
                ConfirmButton.IsEnabled = true;
                return;
            }

            var credentials = new CredentialsModel
            {
                Server = Server.Text,
                Port = port,
                UserName = UserName.Text,
                Password = Password.Text
            };

            try
            {
                if (HTSPConnectionController.Connect(false, credentials.Server, credentials.Port, credentials.UserName, credentials.Password))
                {
                    DataStorageService.SetCredentials(credentials);
                    Navigation.RemovePage(this);
                }
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
                Status.TextColor = Color.Red;
                HTSPConnectionController.Close(true);
            }

            ConfirmButton.IsEnabled = true;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             (sender, arg) =>
             {
                 switch (arg)
                 {
                     case "XF86Back":
                         {

                             ExitModal.IsVisible = true;
                             break;
                         }
                 }
             });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
        protected override bool OnBackButtonPressed()
        {
            return true;
            //return base.OnBackButtonPressed();
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