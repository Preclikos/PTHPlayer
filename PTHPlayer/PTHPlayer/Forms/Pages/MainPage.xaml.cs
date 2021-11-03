using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Enums;
using PTHPlayer.Event;
using PTHPlayer.Event.Models;
using PTHPlayer.Forms.Controls;
using PTHPlayer.Interfaces;
using PTHPlayer.Player.Enums;
using PTHPlayer.VideoPlayer.Models;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        readonly DataService DataStorage;
        readonly EventService EventNotificationService;

        readonly PlayerController VideoPlayerController;
        readonly HTSPController HTSPConnectionController;

        readonly PlayerControl VideoPlayerControl;
        readonly ChannelControl ChannelSelectionControl;
        readonly EPGControl EPGListControl;

        public MainPage(DataService dataStorage, PlayerController videoPlayerController, HTSPController hTSPController, EventService eventNotificationService)
        {

            InitializeComponent();

            DataStorage = dataStorage;

            EventNotificationService = eventNotificationService;

            VideoPlayerController = videoPlayerController;
            HTSPConnectionController = hTSPController;

            VideoPlayerControl = new PlayerControl(DataStorage, VideoPlayerController, HTSPConnectionController, EventNotificationService) { IsVisible = false };
            MainContent.Children.Add(VideoPlayerControl);

            ChannelSelectionControl = new ChannelControl(DataStorage, VideoPlayerController, EventNotificationService) { IsVisible = false };
            MainContent.Children.Add(ChannelSelectionControl);

            EPGListControl = new EPGControl(DataStorage, VideoPlayerController, HTSPConnectionController) { IsVisible = false };
            MainContent.Children.Add(EPGListControl);

            VideoPlayerController.SetSubtitleDisplay(SubtitleImageComponent);

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            EventNotificationService.EventHandler += EventNotificationService_EventHandler;
            VideoPlayerController.PlayerStateChange += PlayerService_StateChange;

            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             (sender, arg) =>
             {
                 if (MainContent.Children.Any(a => a.IsVisible))
                 {
                     return;
                 }

                 switch (arg)
                 {
                     case "Left":
                         {
                             ChannelSelectionControl.IsVisible = true;
                             break;
                         }
                     case "Right":
                         {
                             EPGListControl.IsVisible = true;
                             break;
                         }
                     case "Return":
                         {
                             VideoPlayerControl.IsVisible = true;
                             VideoPlayerControl.Focus();
                             break;
                         }
                     case "XF86RaiseChannel":
                         {
                             VideoPlayerControl.IsVisible = true;
                             VideoPlayerControl.Focus();
                             VideoPlayerControl.ChannelMove(ChannelMoveDirection.Up);

                             break;
                         }
                     case "XF86LowerChannel":
                         {
                             VideoPlayerControl.IsVisible = true;
                             VideoPlayerControl.Focus();
                             VideoPlayerControl.ChannelMove(ChannelMoveDirection.Down);
                             break;
                         }
                     case "XF86Back":
                         {
                             if (MainContent.Children.Any(a => a.IsVisible))
                             {

                                 foreach (var children in MainContent.Children.Where(a => a.IsVisible))
                                 {
                                     children.IsVisible = false;
                                 }
                             }
                             else
                             {
                                 ExitModal.IsVisible = true;
                             }
                             break;
                         }
                 }
             });
        }

        private void PlayerService_StateChange(object sender, PlayerStateChangeEventArgs e)
        {
            switch (e.State)
            {
                case PlayerState.Stop:
                case PlayerState.Paused:
                    {
                        if (DataStorage.SelectedChannelId == -1)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                MainLogo.FadeTo(1, 1000);
                            });
                            if (Loading.Opacity == 1)
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    Loading.FadeTo(0, 200);
                                });
                            }
                        }
                        else
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                Loading.IsAnimationPlaying = true;
                                Loading.FadeTo(1, 200);

                            });
                        }
                        break;
                    }
                case PlayerState.Playing:
                    {
                        if (MainLogo.Opacity != 0)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                MainLogo.FadeTo(0, 1000);
                            });
                        }

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Loading.FadeTo(0, 1000);
                        });
                        break;
                    }
            }
        }

        private void EventNotificationService_EventHandler(object sender, NotificationEventArgs e)
        {
            NotificationArea.GenerateNotification(e.Title, e.Message, e.Id, e.Type);
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
            //return base.OnBackButtonPressed();
        }

        protected override void OnDisappearing()
        {
            EventNotificationService.EventHandler -= EventNotificationService_EventHandler;
            VideoPlayerController.PlayerStateChange -= PlayerService_StateChange;
            MessagingCenter.Unsubscribe<IKeyEventSender, string>(this, "KeyDown");
            base.OnDisappearing();

        }
    }
}