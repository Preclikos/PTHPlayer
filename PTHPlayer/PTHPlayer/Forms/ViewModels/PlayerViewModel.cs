using PTHPlayer.DataStorage.Models;
using System.Windows.Input;

namespace PTHPlayer.Forms.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        private int id = -1;
        private string label;
        private int number;

        private string title;
        private string fullDescription;
        private string description;
        private double progress;
        private EPGModel ePGModel;

        private string time;
        private string startTime;
        private string endTime;

        ICommand playStopCommand;
        ICommand audioSelectionCommand;
        ICommand subtitleSeletionCommand;
        ICommand settingsCommand;

        public int Id
        {
            set { SetProperty(ref id, value); }
            get { return id; }
        }

        public string Label
        {
            set { SetProperty(ref label, value); }
            get { return label; }
        }

        public int Number
        {
            set { SetProperty(ref number, value); }
            get { return number; }
        }

        public string Title
        {
            set { SetProperty(ref title, value); }
            get { return title; }
        }

        public string Description
        {
            set { SetProperty(ref description, value); }
            get { return description; }
        }

        public string FullDescription
        {
            set { SetProperty(ref fullDescription, value); }
            get { return fullDescription; }
        }

        public double Progress
        {
            set { SetProperty(ref progress, value); }
            get { return progress; }
        }

        public EPGModel EPGModel
        {
            set { SetProperty(ref ePGModel, value); }
            get { return ePGModel; }
        }

        public string Time
        {
            set { SetProperty(ref time, value); }
            get { return time; }
        }
        public string StartTime
        {
            set { SetProperty(ref startTime, value); }
            get { return startTime; }
        }
        public string EndTime
        {
            set { SetProperty(ref endTime, value); }
            get { return endTime; }
        }

        public ICommand PlayStopCommand
        {
            set { SetProperty(ref playStopCommand, value); }
            get { return playStopCommand; }
        }
        public ICommand AudioSelectionCommand
        {
            set { SetProperty(ref audioSelectionCommand, value); }
            get { return audioSelectionCommand; }
        }
        public ICommand SubtitleSelectionCommand
        {
            set { SetProperty(ref subtitleSeletionCommand, value); }
            get { return subtitleSeletionCommand; }
        }
        public ICommand SettingsCommand
        {
            set { SetProperty(ref settingsCommand, value); }
            get { return settingsCommand; }
        }
    }
}
