using System;
using UIKit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;

[assembly: ExportRenderer(typeof(Bugzilla60122._60122Image), typeof(_60122ImageRenderer))]

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS
{
	public class _60122ImageRenderer : ImageRenderer
	{
		Bugzilla60122._60122Image _customControl;

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				_customControl = e.NewElement as Bugzilla60122._60122Image;

				if (e.OldElement == null)
				{
					UILongPressGestureRecognizer longp = new UILongPressGestureRecognizer(LongPress); 
					AddGestureRecognizer(longp);
				}
			}
		}

		public void LongPress()
		{
			_customControl?.HandleLongPress(_customControl, new EventArgs());
		}  
	}
}