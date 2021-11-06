using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Event;
using PTHPlayer.Forms.ViewModels;
using PTHPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChannelControl : Grid
    {
        readonly DataService DataStorage;
        readonly PlayerController VideoPlayerController;
        readonly EventService EventNotificationService;
        public ObservableCollection<ChannelViewModel> Items { get; set; }

        public ChannelControl(DataService dataStorage, PlayerController videoPlayerController, EventService eventNotificationService)
        {
            InitializeComponent();

            DataStorage = dataStorage;
            VideoPlayerController = videoPlayerController;
            EventNotificationService = eventNotificationService;
        }

        private void OnAppearing()
        {
            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             (sender, arg) =>
             {
                 if (arg == "XF86Back")
                 {
                     OnDisappearing();
                 }
             });

            var channelView = new List<ChannelViewModel>();

            var channels = DataStorage.GetChannels().OrderBy(o => o.Number);
            var EPGs = DataStorage.GetEPGs();

            var actualDate = DateTime.Now;
            foreach (var channel in channels)
            {
                var newChannel = new ChannelViewModel
                {
                    Id = channel.Id,
                    Label = channel.Label,
                    Number = channel.Number
                };

                if (!String.IsNullOrEmpty(channel.Icon))
                {
                    if (!channel.HasHttpIcon())
                    {
                        string filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), channel.Icon + ".png");
                        newChannel.Image = filepath;
                    }
                    else
                    {
                        newChannel.ImageUrl = channel.Icon;
                    }
                }

                try
                {
                    if (EPGs.Any())
                    {

                        var currentEpg = EPGs.SingleOrDefault(s => s.EventId == channel.EventId);
                        if (currentEpg != null)
                        {
                            newChannel.Title = currentEpg.Title;

                            newChannel.Progress = currentEpg.GetProgress();

                        }

                    }
                }
                catch (Exception ex)
                {
                    EventNotificationService.SendNotification("Channel List Parser", ex.Message);
                }
                channelView.Add(newChannel);
            }

            ChannelListView.ItemsSource = channelView;

            var channelId = DataStorage.SelectedChannelId;
            if (channelId != -1)
            {
                var selecteditem = channelView.SingleOrDefault(s => s.Id == channelId);
                if (selecteditem != null)
                {
                    ChannelListView.SelectedItem = selecteditem;

                }
            }

            ChannelListView.SelectedItem = null;
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            var channelModel = (ChannelViewModel)e.Item;

            VideoPlayerController.Subscription(channelModel.Id);

            DataStorage.SelectedChannelId = channelModel.Id;

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

        private void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<IKeyEventSender, string>(this, "KeyDown");
            this.IsVisible = false;
        }
    }
}
