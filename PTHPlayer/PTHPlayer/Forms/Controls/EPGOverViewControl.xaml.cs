﻿using PTHPlayer.Controllers;
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
    public partial class EPGOverViewControl : Grid
    {
        readonly DataService DataStorage;
        private readonly PlayerController VideoPlayerController;
        readonly HTSPController HTSPConnectionController;
        private List<ChannelModel> Channels = new List<ChannelModel>();
        private List<EPGModel> EPGs = new List<EPGModel>();

        EPGButton focusedButton;
        int FirstChannelId;
        int LastChannelId;
        int TotalEpgCount;

        const int startTimeOverFlow = 60;
        const int scaleFactor = 6;
        const int rowHeight = 100;
        const int titleOffset = 180;
        const int timeOffset = 60;
        const int spacing = 2;
        const int rowCount = 6;

        public EPGOverViewControl(DataService dataStorage, PlayerController videoPlayerController, HTSPController hTSPController)
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

        void FillData(bool fromFirst = false, bool fromLast = false)
        {
            EPGLabel.Children.Clear();
            EPGContent.Children.Clear();

            var actualDate = DateTime.Now;
            ChannelModel selectedChannel = null;

            var channels = new Dictionary<ChannelModel, List<EPGModel>>();

            if (fromFirst)
            {
                var selectedChannels = Channels.SingleOrDefault(w => w.Id == FirstChannelId);
                var downNumber = Channels.Where(w => w.Number < selectedChannels.Number).OrderByDescending(o => o.Number).Take(rowCount + 1);
                foreach (var down in downNumber)
                {
                    channels.Add(down, EPGs.Where(w => w.ChannelId == down.Id && w.End > actualDate).OrderBy(o => o.Start).ToList());
                }
            }
            else if (fromLast)
            {
                var selectedChannels = Channels.SingleOrDefault(w => w.Id == LastChannelId);
                var upNumber = Channels.Where(w => w.Number > selectedChannels.Number).OrderBy(o => o.Number).Take(rowCount + 1);
                foreach (var up in upNumber)
                {
                    channels.Add(up, EPGs.Where(w => w.ChannelId == up.Id && w.End > actualDate).OrderBy(o => o.Start).ToList());
                }
            }
            else
            {
                int selectedChannelId = Channels.FirstOrDefault() == null ? -1 : Channels.OrderBy(o => o.Number).FirstOrDefault().Id;

                if (DataStorage.SelectedChannelId != -1)
                {
                    selectedChannelId = DataStorage.SelectedChannelId;
                }

                selectedChannel = Channels.SingleOrDefault(w => w.Id == selectedChannelId);
                if (selectedChannel == null)
                {
                    return;
                }

                var orderedChannelsForFilter = Channels.OrderBy(o => o.Number);

                var downNumber = orderedChannelsForFilter.Where(w => w.Number < selectedChannel.Number);
                var upNumber = orderedChannelsForFilter.Where(w => w.Number > selectedChannel.Number);

                int halfToTake = rowCount / 2;

                int toUpTake = halfToTake;
                int toDownTake = halfToTake;

                if (downNumber.Count() < halfToTake)
                {
                    toUpTake += halfToTake - downNumber.Count();
                }
                else if (upNumber.Count() < halfToTake)
                {
                    toDownTake += halfToTake - upNumber.Count();
                }


                downNumber = downNumber.OrderByDescending(o => o.Number).Take(toDownTake);
                upNumber = upNumber.Take(toUpTake);

                channels.Add(selectedChannel, EPGs.Where(w => w.ChannelId == selectedChannel.Id && w.End > actualDate).OrderBy(o => o.Start).ToList());

                foreach (var down in downNumber)
                {
                    channels.Add(down, EPGs.Where(w => w.ChannelId == down.Id && w.End > actualDate).OrderBy(o => o.Start).ToList());
                }

                foreach (var up in upNumber)
                {
                    channels.Add(up, EPGs.Where(w => w.ChannelId == up.Id && w.End > actualDate).OrderBy(o => o.Start).ToList());
                }

            }

            var orderedChannels = channels.OrderBy(o => o.Key.Number);

            TotalEpgCount = 0;
            int[] epgCounts = new int[channels.Count];

            int heightOffset = 0;
            foreach (var channel in orderedChannels)
            {
                epgCounts[heightOffset] = channel.Value.Count;
                TotalEpgCount += channel.Value.Count;
                heightOffset++;
            }

            int firstRowWithData = 0;
            int lastRowWithData = 0;

            for (int c = 0; c < epgCounts.Length; c++)
            {
                if (epgCounts[c] > 0)
                {
                    firstRowWithData = c;
                    break;
                }
            }

            for (int c = epgCounts.Length - 1; c >= 0; c--)
            {
                if (epgCounts[c] > 0)
                {
                    lastRowWithData = c;
                    break;
                }
            }


            var filterDate = actualDate - TimeSpan.FromMinutes(startTimeOverFlow);
            /*
            foreach (var channel in channels)
            {
                foreach (var epg in channel.Value)
                {
                    if (epg.Start < filterDate)
                    {
                        filterDate = epg.Start;
                    }
                }
            }*/
            var roundedTime = RoundUp(filterDate, TimeSpan.FromMinutes(30));

            for (int i = 0; i < 16; i++)
            {
                var start = roundedTime - filterDate;

                var lableLayout = new StackLayout();
                lableLayout.Children.Add(new Label { Text = roundedTime.ToString("HH:mm"), MinimumHeightRequest = timeOffset });
                EPGContent.Children.Add(lableLayout, new Rectangle(start.TotalMinutes * scaleFactor, 0, 80, timeOffset), AbsoluteLayoutFlags.None);

                roundedTime += TimeSpan.FromMinutes(30);
            }
            var current = actualDate - filterDate;
            var lineLayout = new StackLayout() { MinimumHeightRequest = timeOffset + (epgCounts.Length * rowHeight), MinimumWidthRequest = 2, BackgroundColor = Color.WhiteSmoke };
            EPGContent.Children.Add(lineLayout, new Rectangle(current.TotalMinutes * scaleFactor, timeOffset, 2, epgCounts.Length * rowHeight), AbsoluteLayoutFlags.None);

            EPGButton buttonToFocus = null;

            heightOffset = 0;

            bool fromFirstSelect = fromFirst;
            bool fromLastSelect = fromLast;

            foreach (var channel in orderedChannels)
            {
                var lableLayout = new StackLayout();
                lableLayout.Children.Add(new Label { Text = channel.Key.Label, MinimumHeightRequest = rowHeight });
                EPGLabel.Children.Add(lableLayout, new Rectangle(0, timeOffset + (heightOffset * rowHeight), titleOffset, rowHeight), AbsoluteLayoutFlags.None);

                foreach (var epg in channel.Value)
                {

                    var start = epg.Start - filterDate;
                    var size = epg.End - epg.Start;

                    if (epg.Start < filterDate)
                    {
                        var needRemoveStart = filterDate - epg.Start;
                        size -= needRemoveStart;
                        start += needRemoveStart;
                    }

                    var eventLayout = new StackLayout();
                    var eventButton = new EPGButton { FontSize = 48, Text = epg.Title, MinimumHeightRequest = rowHeight };

                    if (TotalEpgCount != 0)
                    {

                        if (heightOffset == firstRowWithData)
                        {
                            eventButton.FirstRow = true;
                            FirstChannelId = channel.Key.Id;
                            if (fromLastSelect)
                            {
                                buttonToFocus = eventButton;
                                fromLastSelect = false;
                            }
                        }

                        if (heightOffset == lastRowWithData)
                        {

                            eventButton.LastRow = true;
                            LastChannelId = channel.Key.Id;
                            if (fromFirstSelect)
                            {
                                buttonToFocus = eventButton;
                                fromFirstSelect = false;
                            }
                        }
                    }
                    else
                    {
                        if(heightOffset == 0)
                        {
                            FirstChannelId = channel.Key.Id;
                        }

                        if(heightOffset == epgCounts.Length - 1)
                        {
                            LastChannelId = channel.Key.Id;
                        }
                    }

                    if (selectedChannel != null && epg.EventId == selectedChannel.EventId)
                    {
                        buttonToFocus = eventButton;
                    }

                    eventButton.Clicked += EventButton_Clicked;
                    eventButton.Focused += EventButton_Focused;

                    eventLayout.Children.Add(eventButton);
                    EPGContent.Children.Add(eventLayout, new Rectangle(start.TotalMinutes * scaleFactor, spacing + timeOffset + (heightOffset * rowHeight), (size.TotalMinutes * scaleFactor) - (spacing * 2), rowHeight), AbsoluteLayoutFlags.None);
                }

                heightOffset++;
            }

            if (buttonToFocus != null)
            {
                overFirstOne = false;
                overLastOne = false;
                buttonToFocus.Focus();
                if (fromFirst)
                {
                    overLastOne = true;
                }
                if (fromLast)
                {
                    overFirstOne = true;
                }
            }
            else
            {
                focusedButton = null;
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

        bool overFirstOne = false;
        bool overLastOne = false;

        void OnAppearing()
        {

            Channels = DataStorage.GetChannels();
            EPGs = DataStorage.GetEPGs();

            FillData();

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
                                         if (overFirstOne)
                                         {
                                             FillData(fromFirst: true);
                                         }
                                         else
                                         {
                                             overLastOne = false;
                                             overFirstOne = true;
                                         }
                                     }
                                     else
                                     {
                                         overFirstOne = false;
                                         overLastOne = false;
                                     }
                                 }
                                 else if(TotalEpgCount == 0)
                                 {
                                     FillData(fromFirst: true);
                                 }
                                 break;
                             }
                         case "Down":
                             {
                                 if (focusedButton != null)
                                 {
                                     if (focusedButton.LastRow)
                                     {
                                         if (overLastOne)
                                         {
                                             FillData(fromLast: true);
                                         }
                                         else
                                         {
                                             overFirstOne = false;
                                             overLastOne = true;
                                         }
                                     }
                                     else
                                     {
                                         overFirstOne = false;
                                         overLastOne = false;
                                     }
                                 }
                                 else if (TotalEpgCount == 0)
                                 {
                                     FillData(fromLast: true);
                                 }
                                 break;
                             }
                     }
                 }
             });
        }


        private void OnDisappearing()
        {
            EPGLabel.Children.Clear();
            EPGContent.Children.Clear();

            MessagingCenter.Unsubscribe<IKeyEventSender, string>(this, "KeyDown");
            this.IsVisible = false;
        }
    }
}
