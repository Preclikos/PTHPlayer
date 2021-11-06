using PTHLogger;
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
        readonly EPGControl EPGPlayerControl;
        readonly ChannelControl ChannelSelectionControl;
        readonly EPGOverViewControl EPGListControl;

        private readonly ILogger Logger = LoggerManager.GetInstance().GetLogger("PTHPlayer");
        public MainPage(DataService dataStorage, PlayerController videoPlayerController, HTSPController hTSPController, EventService eventNotificationService)
        {

            InitializeComponent();

            DataStorage = dataStorage;

            EventNotificationService = eventNotificationService;

            VideoPlayerController = videoPlayerController;
            HTSPConnectionController = hTSPController;

            VideoPlayerControl = new PlayerControl(DataStorage, VideoPlayerController, HTSPConnectionController, EventNotificationService) { IsVisible = false };
            MainContent.Children.Add(VideoPlayerControl);

            EPGPlayerControl = new EPGControl(DataStorage, VideoPlayerController, HTSPConnectionController, EventNotificationService) { IsVisible = false };
            MainContent.Children.Add(EPGPlayerControl);

            ChannelSelectionControl = new ChannelControl(DataStorage, VideoPlayerController, EventNotificationService) { IsVisible = false };
            MainContent.Children.Add(ChannelSelectionControl);

            EPGListControl = new EPGOverViewControl(DataStorage, VideoPlayerController, HTSPConnectionController) { IsVisible = false };
            MainContent.Children.Add(EPGListControl);

            VideoPlayerController.SetSubtitleDisplay(SubtitleImageComponent);

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            EventNotificationService.EventHandler += EventNotificationService_EventHandler;
            VideoPlayerController.PlayerStateChange += PlayerService_StateChange;
            VideoPlayerController.SubscriptionCompletedEvent += VideoPlayerController_SubscriptionCompletedEvent;
            VideoPlayerController.SubscriptionStartEvent += VideoPlayerController_SubscriptionStartEvent;


            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             (sender, arg) =>
             {
                 if (MainContent.Children.Any(a => a.IsVisible))
                 {
                     return;
                 }

                 switch (arg)
                 {
                     case "Up":
                         {
                             EPGPlayerControl.IsVisible = true;
                             EPGPlayerControl.Focus();
                             EPGPlayerControl.ChannelMove(ChannelMoveDirection.Up);
                             break;
                         }
                     case "Down":
                         {
                             EPGPlayerControl.IsVisible = true;
                             EPGPlayerControl.Focus();
                             EPGPlayerControl.ChannelMove(ChannelMoveDirection.Down);
                             break;
                         }
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

        bool InSubscription = false;
        private void VideoPlayerController_SubscriptionCompletedEvent(object sender, System.EventArgs e)
        {
            Logger.Info("Player Subscription Completed");
            InSubscription = false;
            ShowLoading(false);
        }

        private void VideoPlayerController_SubscriptionStartEvent(object sender, System.EventArgs e)
        {
            Logger.Info("Player Subscription Start");
            InSubscription = true;
            ShowLoading(true);
            ShowLogo(false);
        }

        private void PlayerService_StateChange(object sender, PlayerStateChangeEventArgs e)
        {
            Logger.Info("Player State changed: " + e.State.ToString());
            switch (e.State)
            {
                case PlayerState.Stop:
                    {
                        if (!InSubscription)
                        {
                            ShowLoading(false);
                            ShowLogo(true);
                        }
                        break;
                    }
                case PlayerState.Paused:
                    {
                        ShowLoading(true);
                        break;
                    }
                case PlayerState.Playing:
                    {
                        ShowLoading(false);
                        ShowLogo(false);
                        break;
                    }
            }
        }

        void ShowLogo(bool display)
        {
            if (display)
            {
                if (MainLogo.Opacity == 0)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MainLogo.FadeTo(1, 500);
                    });
                }
            }
            else
            {
                if (MainLogo.Opacity == 1)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MainLogo.FadeTo(0, 500);
                    });
                }
            }
        }

        void ShowLoading(bool display)
        {
            if(display)
            {
                if(Loading.Opacity == 0)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Loading.IsAnimationPlaying = true;
                        Loading.FadeTo(1, 200);

                    });
                }
            }
            else
            {
                if(Loading.Opacity == 1)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Loading.FadeTo(0, 1000);
                    });
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
            VideoPlayerController.SubscriptionCompletedEvent -= VideoPlayerController_SubscriptionCompletedEvent;
            VideoPlayerController.SubscriptionStartEvent -= VideoPlayerController_SubscriptionStartEvent;
            MessagingCenter.Unsubscribe<IKeyEventSender, string>(this, "KeyDown");
            base.OnDisappearing();

        }
    }
}