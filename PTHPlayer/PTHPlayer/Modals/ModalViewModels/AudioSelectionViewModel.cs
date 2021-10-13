using PTHPlayer.VideoPlayer.Models;
using PTHPlayer.ViewModels;
using System.Collections.Generic;

namespace PTHPlayer.Modals.ModalViewModels
{
    public class AudioSelectionViewModel : ViewModelBase
    {
        private ICollection<AudioConfigModel> audioConfig = new List<AudioConfigModel>();

        public ICollection<AudioConfigModel> AudioConfig
        {
            set { SetProperty(ref audioConfig, value); }
            get { return audioConfig; }
        }
    }
}
