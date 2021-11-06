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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlayerHeader : StackLayout
    {
        readonly DataService DataStorage;
        readonly PlayerController VideoPlayerController;
        readonly HTSPController HTSPConnectionController;
        readonly EventService EventNotificationService;
        readonly Timer RefreshTimer;

        readonly EPGViewModel EPGViewModel = new EPGViewModel();

        private List<ChannelModel> Channels = new List<ChannelModel>();
        private List<EPGModel> EPGs = new List<EPGModel>();

        bool DescriptionScrollerReset = false;
        DateTime EPGStart;
        DateTime EPGStop;

        public PlayerHeader()
        {
            //InitializeComponent();

            this.BindingContext = EPGViewModel;
        }
    }
}
