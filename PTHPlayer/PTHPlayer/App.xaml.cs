using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.HTSP;
using PTHPlayer.Models;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
    {
        public static HTSPClientModel HTSPService;
        public static HTSPListener HTPListener;
        public static DataModel DataService;
        private static int LastChannelId = -1;

        private PlayerController VideoPlayerController;
        private CancellationTokenSource CancellationTokenSource;
        public App()
        {
            InitializeComponent();

        }

        protected override void OnStart()
        {
            DataService = new DataModel();

            HTSPService = new HTSPClientModel();

            VideoPlayerController = new PlayerController(HTSPService);

            HTPListener = new HTSPListener(VideoPlayerController);

            MainPage = new MainPage(VideoPlayerController);

        }

        protected override void OnSleep()
        {
            LastChannelId = App.DataService.SelectedChannelId;

            // Handle when your app sleeps
            //PlayerService.Stop();

            HTSPService.Close();

            //PlayerService.CleanUp();

            CancellationTokenSource.Cancel();
        }

        protected override void OnResume()
        {
            CancellationTokenSource = new CancellationTokenSource();
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
