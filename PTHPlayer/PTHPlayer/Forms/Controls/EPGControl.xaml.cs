using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Models;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Enums;
using PTHPlayer.Event;
using PTHPlayer.Forms.ViewModels;
using PTHPlayer.HTSP.Models;
using PTHPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EPGControl : Grid
    {
        readonly DataService DataStorage;
        readonly PlayerController VideoPlayerController;
        readonly HTSPController HTSPConnectionController;
        readonly EventService EventNotificationService;

        Timer RefreshTimer;

        readonly EPGViewModel EPGViewModel = new EPGViewModel();

        private List<ChannelModel> Channels = new List<ChannelModel>();
        private List<EPGModel> EPGs = new List<EPGModel>();

        bool DescriptionScrollerReset = false;
        DateTime EPGStart;
        DateTime EPGStop;

        public EPGControl(DataService dataStorage, PlayerController videoPlayerController, HTSPController hTSPController, EventService eventNotificationService)
        {
            InitializeComponent();

            DataStorage = dataStorage;

            VideoPlayerController = videoPlayerController;
            HTSPConnectionController = hTSPController;

            EventNotificationService = eventNotificationService;

            this.BindingContext = EPGViewModel;

            RefreshTimer = new Timer(400);
            RefreshTimer.AutoReset = true;

            RefreshTimer.Elapsed += RefreshTimer_Elapsed;
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (EPGStart != null && EPGStop != null)
            {
                var totalTime = EPGStop - EPGStart;
                var currentTime = totalTime - (EPGStop - DateTime.Now);
                if (currentTime.Hours == 0)
                {
                    EPGViewModel.CurrentTime = String.Format("{0:00}:{1:00}", currentTime.Minutes, currentTime.Seconds);
                }
                else
                {
                    EPGViewModel.CurrentTime = String.Format("{0:00}:{1:00}:{2:00}", currentTime.Hours, currentTime.Minutes, currentTime.Seconds);
                }
            }
            var maxScroll = DecriptionScroll.Content.Height - 300;
            if (maxScroll > 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (DecriptionScroll.ScrollY == 0)
                    {
                        DescriptionScrollerReset = false;
                    }
                    if (DecriptionScroll.ScrollY < maxScroll - 1 && !DescriptionScrollerReset)
                    {
                        var scrollTarget = DecriptionScroll.ScrollY + 5;
                        if (scrollTarget > maxScroll)
                        {
                            scrollTarget = maxScroll;
                        }
                        DecriptionScroll.ScrollToAsync(0, scrollTarget, true);
                    }
                    else
                    {
                        DescriptionScrollerReset = true;
                        DecriptionScroll.ScrollToAsync(0, 0, true);
                    }
                });
            }
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
                EPGViewModel.Time = actualDate.ToString("HH:mm");

                var channel = Channels.FirstOrDefault(f => f.Id == id);
                if (channel == null)
                {
                    channel = Channels.OrderBy(o => o.Number).First();
                }

                EPGViewModel.Id = channel.Id;
                EPGViewModel.Number = channel.Number;
                EPGViewModel.Label = channel.Label;

                var epg = EPGs.SingleOrDefault(f => f.EventId == channel.EventId);

                if (epg != null)
                {
                    EPGViewModel.StartTime = epg.Start.ToString("HH:mm");
                    EPGStart = epg.Start;
                    EPGViewModel.EndTime = epg.End.ToString("HH:mm");
                    EPGStop = epg.End;
                    EPGViewModel.Title = epg.Title;
                    EPGViewModel.Description = epg.Summary;
                    EPGViewModel.FullDescription = epg.Description;
                    EPGViewModel.Progress = epg.GetProgress();

                    var totalTime = epg.End - epg.Start;
                    var currentTime = totalTime - (epg.End - DateTime.Now);

                    if (currentTime.Hours == 0)
                    {
                        EPGViewModel.CurrentTime = String.Format("{0:00}:{1:00}", currentTime.Minutes, currentTime.Seconds);
                    }
                    else
                    {
                        EPGViewModel.CurrentTime = String.Format("{0:00}:{1:00}:{2:00}", currentTime.Hours, currentTime.Minutes, currentTime.Seconds);
                    }

                    if (totalTime.Hours == 0)
                    {
                        EPGViewModel.TotalTime = String.Format("{0:00}:{1:00}", totalTime.Minutes, totalTime.Seconds);
                    }
                    else
                    {
                        EPGViewModel.TotalTime = String.Format("{0:00}:{1:00}:{2:00}", totalTime.Hours, totalTime.Minutes, totalTime.Seconds);
                    }
                }
                else
                {
                    EPGViewModel.StartTime = String.Empty;
                    EPGViewModel.EndTime = String.Empty;
                    EPGViewModel.Title = String.Empty;
                    EPGViewModel.Description = String.Empty;
                    EPGViewModel.FullDescription = String.Empty;
                    EPGViewModel.Progress = 0;
                }

                if (EPGViewModel.Description == String.Empty)
                {
                    Separator.IsVisible = false;
                }
                else
                {
                    Separator.IsVisible = true;
                }

                if (EPGViewModel.EndTime == String.Empty)
                {
                    EndAt.IsVisible = false;
                }
                else
                {
                    EndAt.IsVisible = true;
                }

                var nextEpg = EPGs.SingleOrDefault(f => f.EventId == channel.NextEventId);
                if (nextEpg != null)
                {

                    EPGViewModel.NextStart = nextEpg.Start.ToString("HH:mm");
                    EPGViewModel.NextEnd = nextEpg.End.ToString("HH:mm");
                    EPGViewModel.NextTitle = nextEpg.Title;

                }
                else
                {
                    EPGViewModel.NextTitle = String.Empty;
                    EPGViewModel.NextEnd = String.Empty;
                    EPGViewModel.NextStart = String.Empty;
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
                        channelId = ChannelMove(EPGViewModel.Id, moveDirection);
                        break;
                    }
                case ChannelMoveDirection.Down:
                    {
                        channelId = ChannelMove(EPGViewModel.Id, moveDirection);
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
            RefreshTimer.Start();

            Channels = DataStorage.GetChannels();
            EPGs = DataStorage.GetEPGs();

            if (DataStorage.SelectedChannelId != -1)
            {
                EPGViewModel.Id = DataStorage.SelectedChannelId;
            }

            ParseChannelToModel(EPGViewModel.Id);

            HTSPConnectionController.ChannelUpdateEvent += HTSPConnectionController_ChannelUpdateEvent;

            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             (sender, arg) =>
             {
                 if (arg == "XF86Back")
                 {
                     OnDisappearing();
                     return;
                 }

                 if (arg == "Return" || arg == "XF86PlayBack")
                 {
                     if (EPGViewModel.Id == DataStorage.SelectedChannelId)
                     {
                         if (arg == "XF86PlayBack")
                         {
                             DataStorage.SelectedChannelId = -1;
                             VideoPlayerController.UnSubscribe();
                         }
                         return;
                     }
                     VideoPlayerController.Subscription(EPGViewModel.Id);
                     DataStorage.SelectedChannelId = EPGViewModel.Id;
                     return;
                 }

                 if (arg == "Up" || arg == "Down")
                 {
                     switch (arg)
                     {

                         case "Up":
                             {
                                 ChannelMove(ChannelMoveDirection.Up);
                                 break;
                             }
                         case "Down":
                             {
                                 ChannelMove(ChannelMoveDirection.Down);
                                 break;
                             }

                     }
                     return;
                 }
             });
        }

        private void HTSPConnectionController_ChannelUpdateEvent(object sender, ChannelUpdateEventArgs e)
        {
            if (e.ChannelId == EPGViewModel.Id)
            {
                Channels = DataStorage.GetChannels();
                EPGs = DataStorage.GetEPGs();

                ParseChannelToModel(e.ChannelId);
            }
        }

        private void OnDisappearing()
        {
            RefreshTimer.Stop();
            HTSPConnectionController.ChannelUpdateEvent -= HTSPConnectionController_ChannelUpdateEvent;
            MessagingCenter.Unsubscribe<IKeyEventSender, string>(this, "KeyDown");
            this.IsVisible = false;
        }
    }
}
