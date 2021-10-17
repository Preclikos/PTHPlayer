using PTHPlayer.Interfaces;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExitControl : Grid
    {
        public ExitControl()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == "IsVisible")
            {
                switch (this.IsVisible)
                {
                    case true:
                        OnAppearing();
                        break;
                    case false:
                        OnDisappearing();
                        break;
                }
            }
        }

        private void OnAppearing()
        {
            YesButton.Focus();

            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             async (sender, arg) =>
             {
                 if (arg == "XF86Back")
                 {
                     this.IsVisible = false;
                 }
             });
        }

        private void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<IKeyEventSender, string>(this, "KeyDown");
            this.IsVisible = false;
        }

        private void Yes_Clicked(object sender, System.EventArgs e)
        {
            Application.Current.Quit();
        }

        private void No_Clicked(object sender, System.EventArgs e)
        {
            this.IsVisible = false;
        }
    }
}