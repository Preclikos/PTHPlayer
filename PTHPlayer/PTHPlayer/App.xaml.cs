﻿using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.HTSP;
using PTHPlayer.HTSP.Listeners;
using PTHPlayer.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
    {
        public static DataService DataStorageService;
        private static int LastChannelId = -1;

        private PlayerController VideoPlayerController;
        private HTSPController HTSPConnectionController;
        public App()
        {
            InitializeComponent();

        }

        protected override void OnStart()
        {
            DataStorageService = new DataService();

            var HTSPService = new HTSPService();

            VideoPlayerController = new PlayerController(HTSPService);

            var HTPListener = new HTSPListener(VideoPlayerController);

            HTSPConnectionController = new HTSPController(HTSPService, DataStorageService, HTPListener);

            MainPage = new NavigationPage(new MainPage(VideoPlayerController));

        }

        protected override void OnSleep()
        {
            LastChannelId = DataStorageService.SelectedChannelId;

            VideoPlayerController.UnSubscribe(true);

            HTSPConnectionController.Close();
        }

        protected override void OnResume()
        {
            DataStorageService.CleanChannelsAndEPGs();

            if (!DataStorageService.IsCredentialsExist())
            {
                var credentialPage = new CredentialsPage(DataStorageService);
                credentialPage.Disappearing += CredentialPage_Disappearing;
                MainPage.Navigation.PushAsync(credentialPage);
            }
            else
            {
                HTSPConnectionController.Connect();
                if (LastChannelId != -1)
                {
                    VideoPlayerController.Subscription(LastChannelId);

                    DataStorageService.SelectedChannelId = LastChannelId;
                }
            }
        }

        private void CredentialPage_Disappearing(object sender, System.EventArgs e)
        {
            HTSPConnectionController.Connect();
        }
    }
}