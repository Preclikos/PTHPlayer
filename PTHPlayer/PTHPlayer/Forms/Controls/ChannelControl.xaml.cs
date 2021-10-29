using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Forms.ViewModels;
using PTHPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChannelControl : Grid
    {
        DataService DataStorage;
        PlayerController VideoPlayerController;
        public ObservableCollection<ChannelViewModel> Items { get; set; }

        public ChannelControl(DataService dataStorage, PlayerController videoPlayerController)
        {
            InitializeComponent();

            DataStorage = dataStorage;
            VideoPlayerController = videoPlayerController;
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
                var newChannel = new ChannelViewModel();

                newChannel.Id = channel.Id;
                newChannel.Label = channel.Label;
                newChannel.Number = channel.Number;

                if (EPGs.Any())
                {
                    var selectedChannel = EPGs.Where(s => s.ChannelId == channel.Id);

                    if (selectedChannel != null && selectedChannel.Any())
                    {
                        var currentEpg = selectedChannel.SingleOrDefault(s => s.EventId == channel.EventId);
                        if (currentEpg != null)
                        {

                            newChannel.Title = currentEpg.Title;

                            var start = currentEpg.Start.Ticks;
                            var end = currentEpg.End.Ticks;
                            var current = actualDate.Ticks;

                            var range = end - start;
                            var currentOnRange = end - current;

                            var onePercentOnRange = range / (double)100;
                            var currentPercent = currentOnRange / onePercentOnRange;
                            var progressPercent = 1 - currentPercent / (double)100;

                            newChannel.Progress = progressPercent;
                        }
                    }
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
