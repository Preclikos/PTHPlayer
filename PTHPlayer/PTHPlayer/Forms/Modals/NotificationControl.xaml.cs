﻿using PTHPlayer.Event.Enums;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationControl : Grid
    {
        private readonly object NotificationChildren = new object();
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
                    lock (NotificationChildren)
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
                    await Task.Delay(1000);
                }
            });
        }

        public void GenerateNotification(string title, string message, EventId typeId = EventId.Generic, EventType eventType = EventType.Info)
        {
            lock (NotificationChildren)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (typeId != EventId.Generic)
                    {
                        foreach (var notificationChild in NotificationLayout.Children)
                        {
                            var existingNotification = (NotificationItem)notificationChild;
                            if (existingNotification.TypeId == typeId)
                            {

                                existingNotification.UpdateNotification(eventType, message, 5);
                                return;
                            }
                        }
                    }

                    var notification = new NotificationItem(typeId, eventType, title, message, 5);


                    NotificationLayout.Children.Insert(0, notification);
                });
            }
        }
    }
}