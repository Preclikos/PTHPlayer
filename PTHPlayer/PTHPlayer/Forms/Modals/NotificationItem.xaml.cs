﻿using PTHPlayer.Event.Enums;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PTHPlayer.Forms.Modals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationItem : StackLayout
    {
        EventType EventType;

        public EventId TypeId;
        public long TimeOut;

        public NotificationItem(EventId typeId, EventType eventType, string title, string message, int timeOut)
        {
            InitializeComponent();
            TypeId = typeId;
            EventType = eventType;
            TimeOut = (DateTime.Now + TimeSpan.FromSeconds(timeOut)).Ticks;
            Title.Text = title;
            Message.Text = message;
            ChangeIcon(eventType);
        }

        public void UpdateNotification(EventType eventType, string message, int timeOut)
        {
            TimeOut = (DateTime.Now + TimeSpan.FromSeconds(timeOut)).Ticks;
            Message.Text = message;
            if (EventType != eventType)
            {
                EventType = eventType;
                ChangeIcon(eventType);
            }
        }

        void ChangeIcon(EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Info:
                    {
                        Icon.Source = "icons/info.png";
                        break;
                    }
                case EventType.Warning:
                    {
                        Icon.Source = "icons/warning.png";
                        break;
                    }
                case EventType.Error:
                    {
                        Icon.Source = "icons/error.png";
                        break;
                    }
                case EventType.Success:
                    {
                        Icon.Source = "icons/success.png";
                        break;
                    }
                case EventType.Loading:
                    {
                        Icon.Source = "icons/share200.gif";
                        Icon.IsAnimationPlaying = true;
                        break;
                    }
            }
        }
    }
}