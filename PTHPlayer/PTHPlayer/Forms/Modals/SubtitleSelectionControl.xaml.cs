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
    public partial class SubtitleSelectionControl : StackLayout
    {
        readonly PlayerController VideoPlayerController;
        readonly SubtitleSelectionViewModel SubtitleSelectionModel;
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

            var subtitleConfig = (SubtitleViewModel)e.Item;
            SubtitleSelectionModel.SubtitleEnabled = true;
            VideoPlayerController.EnableSubtitleTrack(true, subtitleConfig.Index);
            SubtitleEnable.IsToggled = SubtitleSelectionModel.SubtitleEnabled;
            ((ListView)sender).SelectedItem = null;
        }

        void OnAppearing()
        {
            var subtitleViews = new List<SubtitleViewModel>();
            var subtitles = VideoPlayerController.GetSubtitleConfigs();

            foreach (var subtitle in subtitles)
            {
                var language = ISO639_2.FromAlpha3(subtitle.Language);

                subtitleViews.Add(
                    new SubtitleViewModel
                    {
                        Index = subtitle.Index,
                        Language = language != null ? language.Name : subtitle.Language.ToUpper()
                    });
            }

            SubtitleSelectionModel.SubtitleConfig = subtitleViews;

            SubtitleSelectionModel.SubtitleEnabled = VideoPlayerController.GetSubtitleEnabled();

            SubtitleEnable.IsToggled = SubtitleSelectionModel.SubtitleEnabled;

            var selectedItem = VideoPlayerController.GetSelectedSubtitleConfig();
            if (selectedItem != null)
            {
                var selectedView = subtitleViews.SingleOrDefault(s => s.Index == selectedItem.Index);
                if (selectedView != null)
                {
                    SubtitleSelectionList.SelectedItem = selectedView;
                    SubtitleSelectionList.SelectedItem = null;
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

        private void SubtitleEnable_Toggled(object sender, ToggledEventArgs e)
        {
            VideoPlayerController.EnableSubtitleTrack(e.Value);
        }
    }
}