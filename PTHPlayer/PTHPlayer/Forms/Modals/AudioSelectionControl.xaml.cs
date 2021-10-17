
using PTHPlayer.Controllers;
using PTHPlayer.Forms.Modals.ModalViewModels;
using PTHPlayer.Interfaces;
using PTHPlayer.VideoPlayer.Models;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AudioSelectionControl : StackLayout
    {
        PlayerController VideoPlayerController;

        private AudioSelectionViewModel AudioSelectionModel;
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

            var audioConfig = (AudioConfigModel)e.Item;

            VideoPlayerController.ChangeAudioTrack(audioConfig.Index);

            ((ListView)sender).SelectedItem = null;
        }

        void OnAppearing()
        {

            AudioSelectionModel.AudioConfig = VideoPlayerController.GetAudioConfigs();
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
    }
}