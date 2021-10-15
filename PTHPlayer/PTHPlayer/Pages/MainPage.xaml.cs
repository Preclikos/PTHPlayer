using PTHPlayer.Controllers;
using PTHPlayer.Controls;
using PTHPlayer.Enums;
using PTHPlayer.Interfaces;
using PTHPlayer.VideoPlayer.Enums;
using PTHPlayer.VideoPlayer.Models;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private PlayerController VideoPlayerController;

        private PlayerControl VideoPlayerControl;
        private ChannelControl ChannelSelectionControl;

        public MainPage(PlayerController videoPlayerController)
        {

            InitializeComponent();
            VideoPlayerController = videoPlayerController;

            VideoPlayerControl = new PlayerControl(VideoPlayerController) { IsVisible = false };
            MainContent.Children.Add(VideoPlayerControl);

            ChannelSelectionControl = new ChannelControl(VideoPlayerController) { IsVisible = false };
            MainContent.Children.Add(ChannelSelectionControl);

            VideoPlayerController.SetSubtitleDisplay(SubtitleImageComponent);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            VideoPlayerController.PlayerStateChange += PlayerService_StateChange;

            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             async (sender, arg) =>
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
                case PlayerStates.Stop:
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Loading.IsAnimationPlaying = true;
                            Loading.FadeTo(1, 200);

                        });
                        break;
                    }
                case PlayerStates.Play:
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

        protected override bool OnBackButtonPressed()
        {
            return true;
            //return base.OnBackButtonPressed();
        }

        protected override void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<IKeyEventSender, string>(this, "KeyDown");
            base.OnDisappearing();

        }
    }
}