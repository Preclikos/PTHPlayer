
using PTHPlayer.Controllers;
using PTHPlayer.Forms.Modals.ModalViewModels;
using PTHPlayer.Interfaces;
using PTHPlayer.Player.Models;
using PTHPlayer.VideoPlayer.Models;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SubtitleSelectionControl : StackLayout
    {
        private PlayerController VideoPlayerController;
        private SubtitleSelectionViewModel SubtitleSelectionModel;
        public SubtitleSelectionControl(PlayerController videoPlayerController)
        {
            InitializeComponent();

            VideoPlayerController = videoPlayerController;

            SubtitleSelectionModel = new SubtitleSelectionViewModel();

            SubtitleSelectionList.BindingContext = SubtitleSelectionModel;
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

            var subtitleConfig = (SubtitleConfigModel)e.Item;
            SubtitleSelectionModel.SubtitleEnabled = true;
            VideoPlayerController.EnableSubtitleTrack(true, subtitleConfig.Index);
            SubtitleEnable.IsToggled = SubtitleSelectionModel.SubtitleEnabled;
            ((ListView)sender).SelectedItem = null;
        }

        void OnAppearing()
        {
            SubtitleSelectionModel.SubtitleConfig = VideoPlayerController.GetSubtitleConfigs();
            SubtitleSelectionModel.SubtitleEnabled = VideoPlayerController.GetSubtitleEnabled();

            SubtitleEnable.IsToggled = SubtitleSelectionModel.SubtitleEnabled;

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

        private void SubtitleEnable_Toggled(object sender, ToggledEventArgs e)
        {
            VideoPlayerController.EnableSubtitleTrack(e.Value);
        }
    }
}