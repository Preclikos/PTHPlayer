using System;
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
        }

        public void OnError(object sender, Exception ex)
        {
            //NotificationLayout.Children.Add();
        }
    }
}