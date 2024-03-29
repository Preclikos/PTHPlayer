using ElmSharp;
using PTHLogger.Tizen;
using PTHLogger.Udp;
using PTHPlayer.Interfaces;
using System;
using System.Threading.Tasks;
using Tizen.Applications;
using Tizen.System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;
using Log = Tizen.Log;

namespace PTHPlayer
{

    class Program : FormsApplication, IKeyEventSender
    {

        private const string Tag = "PTHPlayer";

        public static Window ParentMainWindow;

        EcoreEvent<EcoreKeyEventArgs> _keyDown;
        protected override void OnCreate()
        {
            base.OnCreate();

            ParentMainWindow = MainWindow;

            _keyDown = new EcoreEvent<EcoreKeyEventArgs>(EcoreEventType.KeyDown, EcoreKeyEventArgs.Create);
            _keyDown.On += (s, e) =>
            {
                MessagingCenter.Send<IKeyEventSender, string>(this, "KeyDown", e.KeyName);
            };
            LoadApplication(new App());
        }


        private Task WaitForMainWindowResize()
        {
            var tcs = new TaskCompletionSource<bool>();

            var screenSize = GetScreenSize();

            if (MainWindow.Geometry.Size != screenSize)
            {
                void Handler(object sender, EventArgs e)
                {
                    if (MainWindow.Geometry.Size != screenSize)
                        return;
                    MainWindow.Resized -= Handler;
                    tcs.SetResult(true);
                }

                MainWindow.Resized += Handler;
            }
            else
            {
                tcs.SetResult(true);
            }

            return tcs.Task;
        }

        private static ElmSharp.Size GetScreenSize()
        {
            var screenSize = new ElmSharp.Size();

            if (!Information.TryGetValue("http://tizen.org/feature/screen.width", out int width))
                return screenSize;
            if (!Information.TryGetValue("http://tizen.org/feature/screen.height", out int height))
                return screenSize;

            screenSize.Width = width;
            screenSize.Height = height;
            return screenSize;
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs evt)
        {
            if (evt.ExceptionObject is Exception e)
            {
                if (e.InnerException != null)
                    e = e.InnerException;

                Log.Error(Tag, e.Message);
                Log.Error(Tag, e.StackTrace);
            }
            else
            {
                Log.Error(Tag, "Got unhandled exception event: " + evt);
            }
        }

        protected override async void OnAppControlReceived(AppControlReceivedEventArgs e)
        {

            await WaitForMainWindowResize();
        }

        static void Main(string[] args)
        {
            UdpLoggerManager.Configure();
            if (!UdpLoggerManager.IsRunning)
                TizenLoggerManager.Configure();

            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

           

            try
            {
                var app = new Program();

                Xamarin.Forms.Forms.Init(app);
                app.Run(args);
            }
            finally
            {
                if (UdpLoggerManager.IsRunning)
                    UdpLoggerManager.Terminate();
            }
        }
    }
}
