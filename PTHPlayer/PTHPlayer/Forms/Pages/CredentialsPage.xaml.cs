using PTHPlayer.DataStorage.Service;
using PTHPlayer.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CredentialsPage : ContentPage
    {
        DataService DataStorageService;
        public CredentialsPage(DataService dataService)
        {
            InitializeComponent();
            DataStorageService = dataService;
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            var port = int.Parse(Port.Text);

            var credentials = new CredentialsModel
            {
                Server = Server.Text,
                Port = port,
                UserName = UserName.Text,
                Password = Password.Text
            };
            DataStorageService.SetCredentials(credentials);

            Navigation.RemovePage(this);
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