using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportRenderer(typeof(ImageButton), typeof(PTHPlayer.Forms.Components.Renderers.ImageButtonRenderer))]
namespace PTHPlayer.Forms.Components.Renderers
{
    public class ImageButtonRenderer : Xamarin.Forms.Platform.Tizen.ImageButtonRenderer
    {

        protected override void OnElementChanged(ElementChangedEventArgs<ImageButton> e)
        {
            base.OnElementChanged(e);

            Control.SetPartColor("bg_focused", new ElmSharp.Color(255, 255, 255, 75));
        }
    }
}