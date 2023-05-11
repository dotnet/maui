using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.ControlGallery.iOS;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Controls.Platform;
using ObjCRuntime;
using UIKit;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: ExportRenderer(typeof(Bugzilla60122._60122Image), typeof(_60122ImageRenderer))]
#pragma warning restore CS0612 // Type or member is obsolete

namespace Microsoft.Maui.Controls.ControlGallery.iOS
{
	[System.Obsolete]
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