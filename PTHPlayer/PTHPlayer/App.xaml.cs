using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Event;
using PTHPlayer.Forms.Pages;
using PTHPlayer.HTSP;
using PTHPlayer.HTSP.Listeners;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
    {
        private DataService DataStorageService;
        private static int LastChannelId = -1;

        private PlayerController VideoPlayerController;
        private HTSPController HTSPConnectionController;
        public App()
        {
            InitializeComponent();
        }

        protected override void OnStart()
        {
            var eventService = new EventService();

            DataStorageService = new DataService();

            var HTSPService = new HTSPService();

            VideoPlayerController = new PlayerController(HTSPService, eventService);
            HTSPConnectionController = new HTSPController(HTSPService, DataStorageService, eventService);

            var HTPListener = new HTSPListener(DataStorageService, VideoPlayerController, HTSPConnectionController, eventService);

            HTSPConnectionController.SetListener(HTPListener);

            MainPage = new NavigationPage(new MainPage(DataStorageService, VideoPlayerController, HTSPConnectionController, eventService));

        }

        protected override void OnSleep()
        {
            if (DataStorageService.IsCredentialsExist())
            {
                LastChannelId = DataStorageService.SelectedChannelId;

                VideoPlayerController.UnSubscribe(true);

                HTSPConnectionController.Close();
            }
        }

        protected override void OnResume()
        {
            DataStorageService.CleanChannelsAndEPGs();

            if (!DataStorageService.IsCredentialsExist())
            {
                OpenCredentials(false);
            }
            else
            {
                try
                {
                    HTSPConnectionController.Connect(true);
                    if (LastChannelId != -1)
                    {
                        VideoPlayerController.Subscription(LastChannelId);

                        DataStorageService.SelectedChannelId = LastChannelId;
                    }
                }
                catch
                {
                    OpenCredentials(true);
                }
            }
        }
        private void OpenCredentials(bool invalidLogin)
        {
            var credentialPage = new CredentialsPage(DataStorageService, HTSPConnectionController, invalidLogin);
            credentialPage.Disappearing += CredentialPage_Disappearing;
            MainPage.Navigation.PushAsync(credentialPage);
        }

        private void CredentialPage_Disappearing(object sender, System.EventArgs e)
        {
            if (!DataStorageService.IsCredentialsExist())
            {
                Current.Quit();
            }
        }
    }
}
