using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Models;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Enums;
using PTHPlayer.Event;
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
        readonly DataService DataStorage;
        readonly PlayerController VideoPlayerController;
        readonly HTSPController HTSPConnectionController;
        readonly EventService EventNotificationService;
        readonly AudioSelectionControl AudioSelection;
        readonly SubtitleSelectionControl SubtitleSelection;
        readonly SettingControl Setting;

        readonly PlayerViewModel PlayerViewModel = new PlayerViewModel();

        private List<ChannelModel> Channels = new List<ChannelModel>();
        private List<EPGModel> EPGs = new List<EPGModel>();

        public PlayerControl(DataService dataStorage, PlayerController videoPlayerController, HTSPController hTSPController, EventService eventNotificationService)
        {
            InitializeComponent();

            DataStorage = dataStorage;

            VideoPlayerController = videoPlayerController;
            HTSPConnectionController = hTSPController;

            EventNotificationService = eventNotificationService;

            this.BindingContext = PlayerViewModel;

            AudioSelection = new AudioSelectionControl(VideoPlayerController) { IsVisible = false };
            ModalSection.Children.Add(AudioSelection);
            SubtitleSelection = new SubtitleSelectionControl(VideoPlayerController) { IsVisible = false };
            ModalSection.Children.Add(SubtitleSelection);

            Setting = new SettingControl(VideoPlayerController) { IsVisible = false };
            ModalSection.Children.Add(Setting);
        }

        private int ChannelMove(int currentChannel, ChannelMoveDirection channelMove)
        {
            try
            {
                if (Channels.Count == 0)
                {
                    return currentChannel;
                }

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
            catch (Exception ex)
            {
                EventNotificationService.SendNotification("Channel Move", ex.Message);
                return -1;
            }
        }

        private void ParseChannelToModel(int id)
        {
            try
            {
                if (Channels.Count == 0)
                {
                    return;
                }

                var actualDate = DateTime.Now;
                PlayerViewModel.Time = actualDate.ToString("HH:mm");

                var channel = Channels.FirstOrDefault(f => f.Id == id);
                if (channel == null)
                {
                    channel = Channels.OrderBy(o => o.Number).First();
                }

                PlayerViewModel.Id = channel.Id;
                PlayerViewModel.Number = channel.Number;
                PlayerViewModel.Label = channel.Label;
                //PlayerViewModel.EPGModel = channelEPGs;

                if (EPGs.Any(f => f.EventId == channel.EventId))
                {
                    var epg = EPGs.Single(f => f.EventId == channel.EventId);

                    PlayerViewModel.StartTime = epg.Start.ToString("HH:mm");
                    PlayerViewModel.EndTime = epg.End.ToString("HH:mm");
                    PlayerViewModel.Title = epg.Title;
                    PlayerViewModel.Description = epg.Summary;
                    PlayerViewModel.FullDescription = epg.Description;
                    PlayerViewModel.Progress = epg.GetProgress();
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

                if (PlayerViewModel.Id == DataStorage.SelectedChannelId)
                {
                    PlayStopButton.ImageSource = ImageSource.FromFile("icons/stop.png");
                }
                else
                {
                    PlayStopButton.ImageSource = ImageSource.FromFile("icons/multimedia.png");
                }
            }
            catch (Exception ex)
            {
                EventNotificationService.SendNotification("Channel Player Parser", ex.Message);
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
            if (channelId != -1)
            {
                ParseChannelToModel(channelId);
            }
        }

        private void OnAppearing()
        {
            Channels = DataStorage.GetChannels();
            EPGs = DataStorage.GetEPGs();

            if (DataStorage.SelectedChannelId != -1)
            {
                PlayerViewModel.Id = DataStorage.SelectedChannelId;
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
                     PlayStopButton.Focus();
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
                Channels = DataStorage.GetChannels();
                EPGs = DataStorage.GetEPGs();

                ParseChannelToModel(e.ChannelId);
            }
        }

        void Handle_Settings(object sender, EventArgs e)
        {
            HideModals();
            Setting.IsVisible = true;
            Setting.Focus();
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

            if (PlayerViewModel.Id == DataStorage.SelectedChannelId)
            {
                DataStorage.SelectedChannelId = -1;
                VideoPlayerController.UnSubscribe();
                PlayStopButton.ImageSource = ImageSource.FromFile("icons/multimedia.png");
                return;
            }
            VideoPlayerController.Subscription(PlayerViewModel.Id);
            DataStorage.SelectedChannelId = PlayerViewModel.Id;
            PlayStopButton.ImageSource = ImageSource.FromFile("icons/stop.png");
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
