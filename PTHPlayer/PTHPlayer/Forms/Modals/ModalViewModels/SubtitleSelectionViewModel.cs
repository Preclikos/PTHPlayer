using PTHPlayer.Forms.ViewModels;
using PTHPlayer.Player.Models;
using System.Collections.Generic;

namespace PTHPlayer.Forms.Modals.ModalViewModels
{
    public class SubtitleSelectionViewModel : ViewModelBase
    {
        private ICollection<SubtitleViewModel> subtitleConfig = new List<SubtitleViewModel>();

        private bool subtitleEnabled;

        public ICollection<SubtitleViewModel> SubtitleConfig
        {
            set { SetProperty(ref subtitleConfig, value); }
            get { return subtitleConfig; }
        }

        public bool SubtitleEnabled
        {
            set { SetProperty(ref subtitleEnabled, value); }
            get { return subtitleEnabled; }
        }
    }
}
