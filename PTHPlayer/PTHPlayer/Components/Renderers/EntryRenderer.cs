﻿using PTHPlayer.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportRenderer(typeof(Entry), typeof(PTHPlayer.Components.Renderers.EntryRenderer))]
namespace PTHPlayer.Components.Renderers
{
    public class EntryRenderer : Xamarin.Forms.Platform.Tizen.EntryRenderer
    {
        const string _doneKeyName = "Select";
        const string _cancelKeyName = "Cancel";

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            
            if (Control != null)
            {
                Control.KeyDown += Control_KeyDown;

                if (Control is Xamarin.Forms.Platform.Tizen.Native.Entry nentry)
                {
                    nentry.EntryLayoutFocused += OnFocused;
                    nentry.EntryLayoutUnfocused += OnUnfocused;
                }
            }
        }

        private void Control_KeyDown(object sender, ElmSharp.EvasKeyEventArgs e)
        {
            if (e.KeyName == "Select")
            {
                Control.SetFocus(false);
                Device.BeginInvokeOnMainThread(() =>
                {
                    Element.Text = Control.Text;
                    Element.SendCompleted();
                });
            }
            if (e.KeyName == "Cancel")
            {
                Control.HideInputPanel();
            }
        }
    }
}