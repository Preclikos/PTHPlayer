using ElmSharp;
using PTHPlayer.Interfaces;
using System;
using System.Threading.Tasks;
using Tizen.Applications;
using Tizen.System;
using Xamarin.Forms;

namespace PTHPlayer
{
    class Program : global::Xamarin.Forms.Platform.Tizen.FormsApplication, IKeyEventSender
    {
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

        protected override async void OnAppControlReceived(AppControlReceivedEventArgs e)
        {

            await WaitForMainWindowResize();
        }

        static void Main(string[] args)
        {
            
            var app = new Program();
            Forms.Init(app);
            app.Run(args);
        }
    }
}
