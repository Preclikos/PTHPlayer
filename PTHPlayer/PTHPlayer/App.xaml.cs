using PTHLogger;
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
        private readonly ILogger Logger = LoggerManager.GetInstance().GetLogger("PTHPlayer");

        private DataService DataStorage;
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

            DataStorage = new DataService();

            var HTSPService = new HTSPService();

            VideoPlayerController = new PlayerController(DataStorage, HTSPService, eventService);
            HTSPConnectionController = new HTSPController(DataStorage, HTSPService, eventService);

            var HTPListener = new HTSPListener(DataStorage, VideoPlayerController, HTSPConnectionController, eventService);

            HTSPConnectionController.SetListener(HTPListener);

            MainPage = new NavigationPage(new MainPage(DataStorage, VideoPlayerController, HTSPConnectionController, eventService));

        }

        protected override void OnSleep()
        {
            if (DataStorage.IsCredentialsExist())
            {
                LastChannelId = DataStorage.SelectedChannelId;

                VideoPlayerController.UnSubscribe(true);

                HTSPConnectionController.Close();
            }
        }

        protected override void OnResume()
        {
            Logger.Info("Resume");

            DataStorage.CleanChannelsAndEPGs();

            if (!DataStorage.IsCredentialsExist())
            {
                OpenCredentials(false);
            }
            else
            {
                try
                {
                    if (LastChannelId != -1)
                    {
                        DataStorage.SelectedChannelId = LastChannelId;
                    }
                    HTSPConnectionController.Connect(true);
                }
                catch
                {
                    OpenCredentials(true);
                }
            }
        }
        private void OpenCredentials(bool invalidLogin)
        {
            var credentialPage = new CredentialsPage(DataStorage, HTSPConnectionController, invalidLogin);
            credentialPage.Disappearing += CredentialPage_Disappearing;
            MainPage.Navigation.PushAsync(credentialPage);
        }

        private void CredentialPage_Disappearing(object sender, System.EventArgs e)
        {
            if (!DataStorage.IsCredentialsExist())
            {
                Current.Quit();
            }
            else
            {
                HTSPConnectionController.Connect(true);
            }
        }
    }
}
