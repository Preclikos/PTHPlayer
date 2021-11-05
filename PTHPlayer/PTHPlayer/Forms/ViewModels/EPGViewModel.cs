using PTHPlayer.DataStorage.Models;

namespace PTHPlayer.Forms.ViewModels
{
    public class EPGViewModel : ViewModelBase
    {
        private int id = -1;
        private string label;
        private int number;

        private string title;
        private string fullDescription;
        private string description;
        private double progress;

        private string time;
        private string startTime;
        private string endTime;

        private string currentTime;
        private string totalTime;

        private string nextTitle;

        private string nextStartTime;
        private string nextEndTime;

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

        public string CurrentTime
        {
            set { SetProperty(ref currentTime, value); }
            get { return currentTime; }
        }

        public string TotalTime
        {
            set { SetProperty(ref totalTime, value); }
            get { return totalTime; }
        }

        public string NextTitle
        {
            set { SetProperty(ref nextTitle, value); }
            get { return nextTitle; }
        }

        public string NextStart
        {
            set { SetProperty(ref nextStartTime, value); }
            get { return nextStartTime; }
        }

        public string NextEnd
        {
            set { SetProperty(ref nextEndTime, value); }
            get { return nextEndTime; }
        }
    }
}
