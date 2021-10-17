using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Models;
using PTHPlayer.Enums;
using PTHPlayer.Forms.Modals;
using PTHPlayer.Forms.ViewModels;
using PTHPlayer.HTSP.Models;
using PTHPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlayerControl : Grid
    {
        PlayerController VideoPlayerController;
        private HTSPController HTSPConnectionController;

        AudioSelectionControl AudioSelection;
        SubtitleSelectionControl SubtitleSelection;

        PlayerViewModel PlayerViewModel = new PlayerViewModel();
        List<ChannelModel> Channels = new List<ChannelModel>();
        List<EPGModel> EPGs = new List<EPGModel>();

        public PlayerControl(PlayerController videoPlayerController, HTSPController hTSPController)
        {
            InitializeComponent();

            VideoPlayerController = videoPlayerController;
            HTSPConnectionController = hTSPController;

            this.BindingContext = PlayerViewModel;

            AudioSelection = new AudioSelectionControl(VideoPlayerController) { IsVisible = false };
            ModalSection.Children.Add(AudioSelection);
            SubtitleSelection = new SubtitleSelectionControl(VideoPlayerController) { IsVisible = false };
            ModalSection.Children.Add(SubtitleSelection);
        }

        private int ChannelMove(int currentChannel, ChannelMoveDirection channelMove)
        {
            var channel = Channels.FirstOrDefault(f => f.Id == currentChannel);
            var orderedChannels = Channels.OrderBy(o => o.Number);
            if (channel != null)
            {
                var sameNumberChannels = orderedChannels.Where(w => w.Number == channel.Number);
                if (channelMove == ChannelMoveDirection.Up)
                {
                    if (sameNumberChannels.Any(a => a.Id > channel.Id))
                    {
                        return sameNumberChannels.First(a => a.Id > channel.Id).Id;
                    }
                    var nextChannel = orderedChannels.FirstOrDefault(w => w.Number > channel.Number);
                    if (nextChannel != null)
                    {
                        return nextChannel.Id;
                    }
                    return orderedChannels.First().Id;

                }
                if (channelMove == ChannelMoveDirection.Down)
                {
                    if (sameNumberChannels.Any(a => a.Id < channel.Id))
                    {
                        return sameNumberChannels.Last(a => a.Id < channel.Id).Id;
                    }
                    var nextChannel = orderedChannels.LastOrDefault(w => w.Number < channel.Number);
                    if (nextChannel != null)
                    {
                        return nextChannel.Id;
                    }
                    return orderedChannels.Last().Id;

                }
            }
            return orderedChannels.First().Id;
        }


        private void ParseChannelToModel(int id)
        {
            var actualDate = DateTime.Now;
            PlayerViewModel.Time = actualDate.ToString("HH:mm");

            var channel = Channels.FirstOrDefault(f => f.Id == id);
            if (channel == null)
            {
                channel = Channels.OrderBy(o => o.Number).First();
            }

            var channelEPGs = EPGs.Where(f => f.ChannelId == channel.Id);

            PlayerViewModel.Id = channel.Id;
            PlayerViewModel.Number = channel.Number;
            PlayerViewModel.Label = channel.Label;
            //PlayerViewModel.EPGModel = channelEPGs;

            if (channelEPGs != null && channelEPGs.Any(f => f.EventId == channel.EventId))
            {
                var epg = channelEPGs.FirstOrDefault(f => f.EventId == channel.EventId);

                var start = epg.Start.Ticks;
                var end = epg.End.Ticks;
                var current = actualDate.Ticks;

                var range = end - start;
                var currentOnRange = end - current;

                var onePercentOnRange = range / 100;
                var currentPercent = currentOnRange / onePercentOnRange;
                var progressPercent = 1 - currentPercent / (double)100;

                PlayerViewModel.StartTime = epg.Start.ToString("HH:mm");
                PlayerViewModel.EndTime = epg.End.ToString("HH:mm");
                PlayerViewModel.Title = epg.Title;
                PlayerViewModel.Description = epg.Summary;
                PlayerViewModel.FullDescription = epg.Description;
                PlayerViewModel.Progress = progressPercent;
            }
            else
            {
                PlayerViewModel.StartTime = String.Empty;
                PlayerViewModel.EndTime = String.Empty;
                PlayerViewModel.Title = String.Empty;
                PlayerViewModel.Description = String.Empty;
                PlayerViewModel.FullDescription = String.Empty;
                PlayerViewModel.Progress = 0;
            }

            if (PlayerViewModel.Description == String.Empty)
            {
                Separator.IsVisible = false;
            }
            else
            {
                Separator.IsVisible = true;
            }

            if (PlayerViewModel.EndTime == String.Empty)
            {
                EndAt.IsVisible = false;
            }
            else
            {
                EndAt.IsVisible = true;
            }
        }

        void Handle_ChannelClicked(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            var channelModel = (ChannelViewModel)e.Item;

            VideoPlayerController.Subscription(channelModel.Id);

            ((ListView)sender).SelectedItem = null;
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

        public void ChannelMove(ChannelMoveDirection moveDirection)
        {
            int channelId = 0;
            switch (moveDirection)
            {

                case ChannelMoveDirection.Up:
                    {
                        channelId = ChannelMove(PlayerViewModel.Id, moveDirection);
                        break;
                    }
                case ChannelMoveDirection.Down:
                    {
                        channelId = ChannelMove(PlayerViewModel.Id, moveDirection);
                        break;
                    }

            }

            ParseChannelToModel(channelId);
        }

        private void OnAppearing()
        {
            Channels = App.DataStorageService.GetChannels();
            EPGs = App.DataStorageService.GetEPGs();

            if (App.DataStorageService.SelectedChannelId != -1)
            {
                PlayerViewModel.Id = App.DataStorageService.SelectedChannelId;
            }

            ParseChannelToModel(PlayerViewModel.Id);

            HTSPConnectionController.ChannelUpdateEvent += HTSPConnectionController_ChannelUpdateEvent;

            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             async (sender, arg) =>
             {
                 if (ModalSection.Children.Any(a => a.IsVisible) && arg == "XF86Back")
                 {

                     foreach (var child in ModalSection.Children.Where(a => a.IsVisible))
                     {
                         child.IsVisible = false;
                     }

                     return;
                 }

                 else if (arg == "XF86Back")
                 {
                     OnDisappearing();
                     return;
                 }

                 if (ModalSection.Children.Any(a => a.IsVisible))
                 {
                     return;
                 }

                 if (arg == "XF86RaiseChannel" || arg == "XF86LowerChannel")
                 {
                     PlayButton.Focus();
                     switch (arg)
                     {

                         case "XF86RaiseChannel":
                             {
                                 ChannelMove(ChannelMoveDirection.Up);
                                 break;
                             }
                         case "XF86LowerChannel":
                             {
                                 ChannelMove(ChannelMoveDirection.Down);
                                 break;
                             }

                     }
                     return;
                 }

                 if (arg == "Up" || arg == "Down")
                 {
                     switch (arg)
                     {
                         case "Up":
                             {
                                 if (FullDescription.Opacity == 0)
                                 {
                                     await FullDescription.FadeTo(1, 500);
                                 }
                                 break;
                             }
                         case "Down":
                             {
                                 if (FullDescription.Opacity == 1)
                                 {
                                     await FullDescription.FadeTo(0, 500);
                                 }
                                 break;
                             }
                     }
                 }
             });
        }

        private void HTSPConnectionController_ChannelUpdateEvent(object sender, ChannelUpdateEventArgs e)
        {
            if (e.ChannelId == PlayerViewModel.Id)
            {
                Channels = App.DataStorageService.GetChannels();
                EPGs = App.DataStorageService.GetEPGs();

                ParseChannelToModel(e.ChannelId);
            }
        }

        void Handle_AudioSelection(object sender, EventArgs e)
        {
            HideModals();
            AudioSelection.IsVisible = true;
            AudioSelection.Focus();
        }

        void Handle_SubtitleSelection(object sender, EventArgs e)
        {
            HideModals();
            SubtitleSelection.IsVisible = true;
            SubtitleSelection.Focus();
        }

        void Handle_PlayButton(object sender, EventArgs e)
        {
            if (PlayerViewModel.Id == App.DataStorageService.SelectedChannelId)
            {
                return;
            }
            VideoPlayerController.Subscription(PlayerViewModel.Id);
            App.DataStorageService.SelectedChannelId = PlayerViewModel.Id;
        }

        void HideModals()
        {
            FullDescription.Opacity = 0;
            foreach (var child in ModalSection.Children)
            {
                child.IsVisible = false;
            }
        }

        private void OnDisappearing()
        {
            HideModals();
            HTSPConnectionController.ChannelUpdateEvent -= HTSPConnectionController_ChannelUpdateEvent;
            MessagingCenter.Unsubscribe<IKeyEventSender, string>(this, "KeyDown");
            this.IsVisible = false;
        }
    }
}
