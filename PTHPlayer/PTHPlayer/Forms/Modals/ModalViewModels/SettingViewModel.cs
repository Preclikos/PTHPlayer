using PTHPlayer.Forms.ViewModels;

namespace PTHPlayer.Forms.Modals.ModalViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private double cacheSize;

        public double CacheSize
        {
            set { SetProperty(ref cacheSize, value); }
            get { return cacheSize; }
        }
    }
}
