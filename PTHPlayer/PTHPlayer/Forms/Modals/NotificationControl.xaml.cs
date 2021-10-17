using PTHPlayer.Event.Enums;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationControl : Grid
    {
        public NotificationControl()
        {
            InitializeComponent();
            CleanUpTask();
        }

        void CleanUpTask()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var timeOut = DateTime.Now.Ticks;
                        if (NotificationLayout.Children.Count > 0)
                        {
                            foreach (var notificationChild in NotificationLayout.Children)
                            {
                                var notification = (NotificationItem)notificationChild;
                                if (notification.TimeOut < timeOut)
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        NotificationLayout.Children.Remove(notificationChild);
                                    });
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                    await Task.Delay(1000);
                }
            });
        }

        public void GenerateNotification(string title, string message, EventId typeId = EventId.Generic, EventType eventType = EventType.Info)
        {
            if (typeId != EventId.Generic)
            {
                foreach (var notificationChild in NotificationLayout.Children)
                {
                    var existingNotification = (NotificationItem)notificationChild;
                    if (existingNotification.TypeId == typeId)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            existingNotification.UpdateNotification(message, 5);
                        });
                        return;
                    }
                }
            }

            var notification = new NotificationItem(typeId, title, message, 5);

            Device.BeginInvokeOnMainThread(() =>
            {
                NotificationLayout.Children.Add(notification);
            });
        }
    }
}