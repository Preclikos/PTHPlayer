using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Models;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Forms.Components;
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
    public partial class EPGControl : Grid
    {
        readonly DataService DataStorage;
        private readonly PlayerController VideoPlayerController;
        readonly HTSPController HTSPConnectionController;
        private List<ChannelModel> Channels = new List<ChannelModel>();
        private List<EPGModel> EPGs = new List<EPGModel>();

        EPGButton focusedButton;
        int FirstChannelId;
        int LastChannelId;

        const int scaleFactor = 6;
        const int rowHeight = 100;
        const int titleOffset = 180;
        const int timeOffset = 60;
        const int spacing = 2;

        public EPGControl(DataService dataStorage, PlayerController videoPlayerController, HTSPController hTSPController)
        {
            InitializeComponent();

            DataStorage = dataStorage;
            VideoPlayerController = videoPlayerController;
            HTSPConnectionController = hTSPController;

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

        void fillData(bool fromFirst = false, bool fromLast = false)
        {
            EPGLabel.Children.Clear();
            EPGContent.Children.Clear();

            var actualDate = DateTime.Now;
            var roundedTime = RoundUp(actualDate, TimeSpan.FromMinutes(30));

            for (int i = 0; i < 8; i++)
            {
                var start = roundedTime - actualDate;

                var lableLayout = new StackLayout();
                lableLayout.Children.Add(new Label { Text = roundedTime.ToString("HH:mm"), MinimumHeightRequest = timeOffset });
                EPGContent.Children.Add(lableLayout, new Rectangle(start.TotalMinutes * scaleFactor, 0, 80, timeOffset), AbsoluteLayoutFlags.None);

                roundedTime = roundedTime + TimeSpan.FromMinutes(30);
            }

            var channels = new List<ChannelModel>();
            int heightOffset = 0;

            if (fromFirst)
            {
                var selectedChannels = Channels.SingleOrDefault(w => w.Id == FirstChannelId);
                var downNumber = Channels.Where(w => w.Number < selectedChannels.Number).OrderBy(o => o.Number).Take(7);
            }
            else if (fromLast)
            {
                var selectedChannels = Channels.SingleOrDefault(w => w.Id == LastChannelId);
                var upNumber = Channels.Where(w => w.Number > selectedChannels.Number).OrderBy(o => o.Number).Take(7);
                channels.AddRange(upNumber);
            }
            else
            {
                int selectedChannel = Channels.FirstOrDefault() == null ? -1 : Channels.FirstOrDefault().Id;

                if (DataStorage.SelectedChannelId != -1)
                {
                    selectedChannel = DataStorage.SelectedChannelId;
                }

                
                var selectedChannels = Channels.SingleOrDefault(w => w.Id == selectedChannel);
                if (selectedChannels == null)
                {
                    return;
                }

                var downNumber = Channels.Where(w => w.Number < selectedChannels.Number).OrderBy(o => o.Number).Take(3);
                int upCount = 3;
                if (downNumber.Count() != 3)
                {
                    upCount = upCount + (3 - downNumber.Count());
                }
                var upNumber = Channels.Where(w => w.Number > selectedChannels.Number).OrderBy(o => o.Number).Take(upCount);


                channels.Add(selectedChannels);
                channels.AddRange(downNumber);
                channels.AddRange(upNumber);
            }

            foreach (var channel in channels.OrderBy(o => o.Number))
            {
                var lableLayout = new StackLayout();
                lableLayout.Children.Add(new Label { TextColor = Color.Orange, Text = channel.Label, MinimumHeightRequest = rowHeight });
                EPGLabel.Children.Add(lableLayout, new Rectangle(0, timeOffset + (heightOffset * rowHeight), titleOffset, rowHeight), AbsoluteLayoutFlags.None);

                var singleRow = EPGs.Where(w => w.ChannelId == channel.Id && w.End > DateTime.Now).OrderBy(o => o.Start);

                foreach (var epg in singleRow)
                {

                    var start = epg.Start - DateTime.Now;

                    var size = epg.End - epg.Start;

                    if (epg.Start < DateTime.Now)
                    {
                        var needRemoveStart = DateTime.Now - epg.Start;
                        size = size - needRemoveStart;
                        start = start + needRemoveStart;
                    }

                    var eventLayout = new StackLayout();
                    var eventButton = new EPGButton { TextColor = Color.Red, Text = epg.Title, MinimumHeightRequest = rowHeight };

                    if(heightOffset == 0)
                    {
                        eventButton.FirstRow = true;
                        FirstChannelId = channel.Id;
                    }

                    if (heightOffset == 6)
                    {
                        eventButton.LastRow = true;
                        LastChannelId = channel.Id;
                    }

                    eventButton.Clicked += EventButton_Clicked;
                    eventButton.Focused += EventButton_Focused;

                    eventLayout.Children.Add(eventButton);
                    EPGContent.Children.Add(eventLayout, new Rectangle(start.TotalMinutes * scaleFactor, spacing + timeOffset + (heightOffset * rowHeight), (size.TotalMinutes * scaleFactor) - (spacing * 2), rowHeight), AbsoluteLayoutFlags.None);
                }

                heightOffset++;
            }
        }

        private void EventButton_Focused(object sender, FocusEventArgs e)
        {
            focusedButton = (EPGButton)sender;
        }

        private void EventButton_Clicked(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
        }

        void OnAppearing()
        {

            Channels = DataStorage.GetChannels();
            EPGs = DataStorage.GetEPGs();

            fillData();

            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             (sender, arg) =>
             {

                 if (arg == "XF86Back")
                 {
                     OnDisappearing();
                     return;
                 }

                 if (arg == "Up" || arg == "Down")
                 {
                     switch (arg)
                     {
                         case "Up":
                             {
                                 if (focusedButton != null)
                                 {
                                     if (focusedButton.FirstRow)
                                     {
                                         fillData(fromFirst: true);
                                     }
                                 }
                                 break;
                             }
                         case "Down":
                             {
                                 if(focusedButton != null)
                                 {
                                     if(focusedButton.LastRow)
                                     {
                                         fillData(fromLast: true);
                                     }
                                 }
                                 break;
                             }
                             //fillData();
                     }
                 }
             });
        }


        private void OnDisappearing()
        {
            //EPGGrid.Children.Clear();
            MessagingCenter.Unsubscribe<IKeyEventSender, string>(this, "KeyDown");
            this.IsVisible = false;
        }
    }
}
