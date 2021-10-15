using PTHPlayer.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CredentialsPage : ContentPage
    {
        public CredentialsPage()
        {
            InitializeComponent();

            
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             async (sender, arg) =>
             {
             if (arg == "Cancel" || arg == "Select")
                {
                     this.Focus();
                }  
             });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private void Port_Completed(object sender, System.EventArgs e)
        {

        }
    }
}