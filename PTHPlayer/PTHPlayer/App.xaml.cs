using PTHPlayer.Controllers;
using PTHPlayer.Controls;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.HTSP;
using PTHPlayer.HTSP.Listeners;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
    {
        public static HTSPService HTSPService;
        public static HTSPListener HTPListener;
        public static DataService DataService;
        private static int LastChannelId = -1;

        private PlayerController VideoPlayerController;
        private HTSPController HTSPConnectionController;
        public App()
        {
            InitializeComponent();

        }

        protected override void OnStart()
        {
            DataService = new DataService();

            HTSPService = new HTSPService();

            VideoPlayerController = new PlayerController(HTSPService);

            HTPListener = new HTSPListener(VideoPlayerController);

            HTSPConnectionController = new HTSPController(HTSPService, DataService, HTPListener);

            MainPage = new NavigationPage(new MainPage(VideoPlayerController));

        }

        protected override void OnSleep()
        {
            LastChannelId = App.DataService.SelectedChannelId;

            VideoPlayerController.UnSubscribe(true);

            HTSPService.Close();
        }

        protected override void OnResume()
        {

            if (!DataService.IsCredentialsExist())
            {
                MainPage.Navigation.PushAsync(new CredentialsPage());
            }
            /*
                var credentials = DataService.GetCredentials();

                if (HTSPService.NeedRestart())
                {

                    HTSPService.Open(credentials.Server, credentials.Port, HTPListener);

                    HTSPService.Login(credentials.UserName, credentials.Password);

                    HTSPService.EnableAsyncMetadata();

                    if (LastChannelId != -1)
                    {


                        VideoPlayerController.Subscription(LastChannelId);

                        App.DataService.SelectedChannelId = LastChannelId;
                    }
                }*/

        }
    }
}
