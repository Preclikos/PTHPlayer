using PTHPlayer.Controllers;
using PTHPlayer.DataStorage;
using PTHPlayer.Forms.Modals.ModalViewModels;
using PTHPlayer.Interfaces;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingControl : StackLayout
    {
        readonly SettingViewModel SettingModel = new SettingViewModel();
        readonly IDataStorage NativeDataService;

        public SettingControl(PlayerController videoPlayerController)
        {
            InitializeComponent();

            NativeDataService = DependencyService.Get<IDataStorage>();

            this.BindingContext = SettingModel;
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

        void OnAppearing()
        {
            FillData();

            MessagingCenter.Subscribe<IKeyEventSender, string>(this, "KeyDown",
             (sender, arg) =>
             {

                 if (arg == "XF86Back")
                 {
                     OnDisappearing();

                 }
             });
        }

        void FillData()
        {
            SettingModel.CacheSize = CalculateCacheSize();
        }

        double CalculateCacheSize()
        {
            string iconDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "imagecache");
            var files = Directory.GetFiles(iconDirectory);

            long Size = 0;

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                Size = Size + fileInfo.Length;
            }

            var TotalSize = (double)Size / (double)1024 / (double)1024; //kB

            return TotalSize;
        }

        void OnDisappearing()
        {
            //this.IsVisible = false;
        }

        private void CredentialsClear_Clicked(object sender, System.EventArgs e)
        {
            NativeDataService.ClearCredentials();
            Application.Current.Quit();
        }
    }
}