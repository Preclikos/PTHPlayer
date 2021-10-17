using PTHPlayer.Event.Enums;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationItem : StackLayout
    {
        public EventId TypeId;
        public long TimeOut;
        public NotificationItem(EventId typeId, string title, string message, int timeOut)
        {
            InitializeComponent();
            TypeId = typeId;
            TimeOut = (DateTime.Now + TimeSpan.FromSeconds(timeOut)).Ticks;
            Title.Text = title;
            Message.Text = message;
        }

        public void UpdateNotification(string message, int timeOut)
        {
            TimeOut = (DateTime.Now + TimeSpan.FromSeconds(timeOut)).Ticks;
            Message.Text = message;
        }
    }
}