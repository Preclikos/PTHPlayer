﻿using PTHPlayer.Components;
using PTHPlayer.Components.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportRenderer(typeof(PlayerButton), typeof(PlayerButtonRenderer))]
namespace PTHPlayer.Components.Renderers
{
    class PlayerButtonRenderer : ButtonRenderer
    {
        public PlayerButtonRenderer() : base()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            
            Control.SetPartColor("bg_focused", new ElmSharp.Color(255, 255, 255, 75));

        }
        protected override void UpdateThemeStyle()
        {
            base.UpdateThemeStyle();
        }
    }
}