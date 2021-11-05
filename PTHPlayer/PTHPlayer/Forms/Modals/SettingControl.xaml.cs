
using PTHPlayer.Controllers;
using PTHPlayer.DataStorage;
using PTHPlayer.Forms.Modals.ModalViewModels;
using PTHPlayer.Interfaces;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingControl : StackLayout
    {
        readonly SettingViewModel SettingModel;
        readonly IDataStorage NativeDataService;

        public SettingControl(PlayerController videoPlayerController)
        {
            InitializeComponent();

            SettingModel = new SettingViewModel();

            NativeDataService = DependencyService.Get<IDataStorage>();
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

        void OnAppearing()
        {
            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             (sender, arg) =>
             {

                 if (arg == "XF86Back")
                 {
                     OnDisappearing();

                 }
             });
        }

        void OnDisappearing()
        {
            //this.IsVisible = false;
        }

        private void CredentialsClear_Clicked(object sender, System.EventArgs e)
        {
            NativeDataService.ClearCredentials();
            Application.Current.Quit();
        }
    }
}