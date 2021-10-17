using PTHPlayer.VideoPlayer.Player;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PTHPlayer.Subtitles.Player
{
    public class SubtitlePlayer
    {
        private Image SubtitleDisplayImage { get; set; }
        private CancellationTokenSource CancellationToken { get; set; }
        private PlayerService PlayerService { get; }
        public SubtitlePlayer(PlayerService playerService)
        {
            PlayerService = playerService;
        }

        public void SetDisplay(Image subtitleDisplayImage)
        {
            SubtitleDisplayImage = subtitleDisplayImage;
        }

        public void Start()
        {
            CancellationToken = new CancellationTokenSource();
            var cancellationToken = CancellationToken.Token;
            _ = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {

                        if (!PlayerService.SubtitleEnabled() && SubtitleDisplayImage != null)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                SubtitleDisplayImage.Source = null;
                            });
                            await Task.Delay(2000);
                            continue;
                        }

                        var currentPlayerTime = PlayerService.GetPlayerTime();

                        if (currentPlayerTime == 0)
                        {
                            await Task.Delay(2000);
                            continue;
                        }

                        var subtitlesImage = PlayerService.SubtitleElemnts;
                        if (subtitlesImage.Count == 0)
                        {
                            await Task.Delay(50);
                            continue;
                        }

                        var selectedImage = subtitlesImage.Dequeue();

                        var displayPts = selectedImage.Pts / 1000000;

                        var delayToShow = displayPts - currentPlayerTime;
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                if (delayToShow > 0)
                                {
                                    await Task.Delay((int)delayToShow, cancellationToken);
                                    if (cancellationToken.IsCancellationRequested)
                                    {
                                        return;
                                    }
                                }

                                if (selectedImage != null)
                                {
                                    if (!selectedImage.Empty)
                                    {
                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            var byteData = selectedImage.Data.ToArray();
                                            var imageSource = ImageSource.FromStream(() =>
                                            {
                                                var imageStream = new MemoryStream();
                                                imageStream.Write(byteData, 0, byteData.Length);
                                                imageStream.Position = 0;
                                                return imageStream;
                                            });
                                            SubtitleDisplayImage.Source = imageSource;
                                        });
                                    }
                                    else
                                    {

                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            SubtitleDisplayImage.Source = null;
                                        });

                                    }
                                }
                            }
                            catch
                            {

                            }
                        }, cancellationToken);
                    }
                    catch
                    {
                    }

                }
            }, cancellationToken);
        }

        public void Stop()
        {
            if (CancellationToken != null && !CancellationToken.IsCancellationRequested)
            {
                CancellationToken.Cancel();
            }
        }
    }
}
