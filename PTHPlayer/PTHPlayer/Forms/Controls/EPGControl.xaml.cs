﻿using PTHPlayer.Controllers;
using PTHPlayer.DataStorage.Models;
using PTHPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EPGControl : Grid
    {
        PlayerController VideoPlayerController;
        private HTSPController HTSPConnectionController;

        List<ChannelModel> Channels = new List<ChannelModel>();
        List<EPGModel> EPGs = new List<EPGModel>();

        public EPGControl(PlayerController videoPlayerController, HTSPController hTSPController)
        {
            InitializeComponent();

            VideoPlayerController = videoPlayerController;
            HTSPConnectionController = hTSPController;


            for (int row = 0; row < 5; row++)
            {
                EPGGrid.RowDefinitions.Add(new RowDefinition() { Height = 120 });
            }
            for (int column = 0; column < 24; column++)
            {
                EPGGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = 60 });
            }


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

        private void OnAppearing()
        {
            Channels = App.DataStorageService.GetChannels();
            EPGs = App.DataStorageService.GetEPGs();

            var random = new Random();

            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 24; column++)
                {
                    
                    var color1 = random.Next(254);
                    var color2 = random.Next(254);
                    var color3 = random.Next(254);
                    var layout = new Button() { BackgroundColor = new Color(color1, color2, color3) };
                    EPGGrid.Children.Add(layout, column, row);
                }
            }

            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             async (sender, arg) =>
             {

                 if (arg == "XF86Back")
                 {
                     OnDisappearing();
                     return;
                 }

                 if (arg == "XF86RaiseChannel" || arg == "XF86LowerChannel")
                 {
                     switch (arg)
                     {

                         case "XF86RaiseChannel":
                             {
                                 break;
                             }
                         case "XF86LowerChannel":
                             {
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
                                 break;
                             }
                         case "Down":
                             {
                                 break;
                             }
                     }
                 }
             });
        }


        private void OnDisappearing()
        {
            EPGGrid.Children.Clear();
            MessagingCenter.Unsubscribe<IKeyEventSender, string>(this, "KeyDown");
            this.IsVisible = false;
        }
    }
}