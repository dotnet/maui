using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tizen.Multimedia;

namespace Xamarin.Forms.Platform.Tizen
{
	public class MediaElementRenderer : ViewRenderer<MediaElement, MediaView>
	{
		public MediaElementRenderer()
		{
			Xamarin.Forms.MediaElement.VerifyMediaElementFlagEnabled(nameof(MediaElementRenderer));
		}
		protected override void OnElementChanged(ElementChangedEventArgs<MediaElement> e)
		{
			if (Control == null)
			{
				SetNativeControl(new MediaView(Forms.NativeParent));
				
			}
			base.OnElementChanged(e);
		}
	}
}
