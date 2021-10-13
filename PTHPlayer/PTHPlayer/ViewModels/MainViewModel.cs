using PTHPlayer.DataStorage.Models;
using System.Collections.Generic;

namespace PTHPlayer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            ApplicationCollection = new List<ChannelModel>();
        }

        private List<ChannelModel> _applicationCollection;

        public List<ChannelModel> ApplicationCollection
        {
            set { SetProperty(ref _applicationCollection, value); }
            get { return _applicationCollection; }
        }
    }
}
