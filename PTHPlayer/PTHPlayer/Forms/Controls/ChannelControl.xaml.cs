using PTHPlayer.Controllers;
using PTHPlayer.Forms.ViewModels;
using PTHPlayer.Interfaces;
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
        PlayerController VideoPlayerController;
        public ObservableCollection<ChannelViewModel> Items { get; set; }

        public ChannelControl(PlayerController videoPlayerController)
        {
            InitializeComponent();

            VideoPlayerController = videoPlayerController;
        }

        private void OnAppearing()
        {
            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             async (sender, arg) =>
             {
                 if (arg == "XF86Back")
                 {
                     OnDisappearing();
                 }
             });

            var channelView = new List<ChannelViewModel>();

            var channels = App.DataStorageService.GetChannels().OrderBy(o => o.Number);
            var EPGs = App.DataStorageService.GetEPGs();
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
                        }
                    }
                }
                channelView.Add(newChannel);
            }

            ChannelListView.ItemsSource = channelView;

            var channelId = App.DataStorageService.SelectedChannelId;
            if (channelId != -1)
            {
                var selecteditem = channelView.SingleOrDefault(s => s.Id == channelId);
                if (selecteditem != null)
                {
                    ChannelListView.SelectedItem = selecteditem;
                }
            }
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            var channelModel = (ChannelViewModel)e.Item;

            VideoPlayerController.Subscription(channelModel.Id);

            App.DataStorageService.SelectedChannelId = channelModel.Id;

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
