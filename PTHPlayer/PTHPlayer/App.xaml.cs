using PTHPlayer.Controllers;
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
        public static DataModel DataService;
        private static int LastChannelId = -1;

        private PlayerController VideoPlayerController;
        public App()
        {
            InitializeComponent();

        }

        protected override void OnStart()
        {
            DataService = new DataModel();

            HTSPService = new HTSPService();

            VideoPlayerController = new PlayerController(HTSPService);

            HTPListener = new HTSPListener(VideoPlayerController);

            MainPage = new MainPage(VideoPlayerController);

        }

        protected override void OnSleep()
        {
            LastChannelId = App.DataService.SelectedChannelId;

            // Handle when your app sleeps
            //PlayerService.Stop();

            VideoPlayerController.UnSubscribe(true);
            HTSPService.Close();

            //VideoPlayerController.UnSubscribe();



            //PlayerService.CleanUp();
        }

        protected override void OnResume()
        {
            //VideoPlayerController.ResetState();

            if (HTSPService.NeedRestart())
            {
                DataService = new DataModel();

                HTSPService.Open("192.168.1.210", 9982, HTPListener);

                HTSPService.Login("test", "test");

                HTSPService.EnableAsyncMetadata();

                if (LastChannelId != -1)
                {


                    VideoPlayerController.Subscription(LastChannelId);

                    App.DataService.SelectedChannelId = LastChannelId;
                }
            }
        }
    }
}
