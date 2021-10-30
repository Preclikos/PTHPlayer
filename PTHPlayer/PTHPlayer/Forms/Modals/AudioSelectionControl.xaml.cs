using PTHPlayer.Controllers;
using PTHPlayer.Forms.Languages;
using PTHPlayer.Forms.Modals.ModalViewModels;
using PTHPlayer.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AudioSelectionControl : StackLayout
    {
        readonly PlayerController VideoPlayerController;

        readonly AudioSelectionViewModel AudioSelectionModel;
        public AudioSelectionControl(PlayerController videoPlayerController)
        {
            InitializeComponent();

            VideoPlayerController = videoPlayerController;

            AudioSelectionModel = new AudioSelectionViewModel();

            AudioSelectionList.BindingContext = AudioSelectionModel;
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

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            var audioConfig = (AudioViewModel)e.Item;

            VideoPlayerController.ChangeAudioTrack(audioConfig.Index);

            ((ListView)sender).SelectedItem = null;
        }

        void OnAppearing()
        {
            VideoPlayerController.GetAudioConfigs();

            var audioViews = new List<AudioViewModel>();
            var audios = VideoPlayerController.GetAudioConfigs();

            foreach (var audio in audios)
            {
                var language = ISO639_2.FromAlpha3(audio.Language);
                var Language = language != null ? language.Name : audio.Language.ToUpper();
                var Channels = ParseChannels(audio.Channels);
                var Codec = ((AudioCodecs)audio.Codec).ToString();

                audioViews.Add(
                    new AudioViewModel
                    {
                        Index = audio.Index,
                        Language = Language,
                        Channels = Channels,
                        Codec = Codec,
                        Label = Language + " (" + Channels + ")  - " + Codec
                    });
            }

            AudioSelectionModel.AudioConfig = audioViews;

            var selectedItem = VideoPlayerController.GetSelectedAudioConfig();
            if (selectedItem != null)
            {
                var selectedView = audioViews.SingleOrDefault(s => s.Index == selectedItem.Index);
                if (selectedView != null)
                {
                    AudioSelectionList.SelectedItem = selectedItem;
                    AudioSelectionList.SelectedItem = null;
                }
            }

            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             (sender, arg) =>
             {

                 if (arg == "XF86Back")
                 {
                     OnDisappearing();

                 }
             });
        }

        void OnDisappearing()
        {
            //this.IsVisible = false;
        }

        enum AudioCodecs
        {
            MPEG2 = 2,
            AC3 = 4,
            AAC = 1,
            EAC3 = 5,
            VORBIS = 6
        }

        string ParseChannels(int channels)
        {
            switch (channels)
            {
                case 2:
                    {
                        return "2.0";
                    }
                case 3:
                    {
                        return "2.1";
                    }
                case 6:
                    {
                        return "5.1";
                    }
                case 8:
                    {
                        return "7.1";
                    }
                default:
                    {
                        return "Unspec";
                    }
            }
        }
    }
}