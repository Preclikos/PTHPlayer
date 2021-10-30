using PTHPlayer.Forms.ViewModels;
using PTHPlayer.Player.Models;
using System.Collections.Generic;

namespace PTHPlayer.Forms.Modals.ModalViewModels
{
    public class AudioSelectionViewModel : ViewModelBase
    {
        private ICollection<AudioViewModel> audioConfig = new List<AudioViewModel>();

        public ICollection<AudioViewModel> AudioConfig
        {
            set { SetProperty(ref audioConfig, value); }
            get { return audioConfig; }
        }
    }
}
