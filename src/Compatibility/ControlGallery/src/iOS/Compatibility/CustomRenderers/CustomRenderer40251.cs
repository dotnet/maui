using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.ControlGallery.iOS;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Controls.Platform;
using ObjCRuntime;
using UIKit;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: ExportRenderer(typeof(Button), typeof(CustomRenderer40251))]
#pragma warning restore CS0612 // Type or member is obsolete
namespace Microsoft.Maui.Controls.ControlGallery.iOS
{
	[System.Obsolete]
	public class CustomRenderer40251 : ButtonRenderer
	{
		Dictionary<string, object> originalValues = new Dictionary<string, object>();

		public CustomRenderer40251()
		{
			if (TestPage40251.Arg == "TitleColor")
			{
				originalValues.Add("TitleColor", UIButton.Appearance.TitleColor(UIControlState.Normal));
				UIButton.Appearance.SetTitleColor(UIColor.Red, UIControlState.Normal);
			}
			else if (TestPage40251.Arg == "TitleShadowColor")
			{
				originalValues.Add("TitleShadowColor", UIButton.Appearance.TitleShadowColor(UIControlState.Normal));
				UIButton.Appearance.SetTitleShadowColor(UIColor.White, UIControlState.Normal);
			}
			else if (TestPage40251.Arg == "BackgroundImage")
			{
				originalValues.Add("BackgroundImage", UIButton.Appearance.BackgroundImageForState(UIControlState.Normal));
				UIButton.Appearance.SetBackgroundImage(new UIImage("Intranet-icon.png"), UIControlState.Normal);
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control != null)
					Control.TitleLabel.ShadowOffset = new CoreGraphics.CGSize(2, 2);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (TestPage40251.Arg == "TitleColor")
					UIButton.Appearance.SetTitleColor(originalValues["TitleColor"] as UIColor, UIControlState.Normal);
				else if (TestPage40251.Arg == "TitleShadowColor")
					UIButton.Appearance.SetTitleShadowColor(originalValues["TitleShadowColor"] as UIColor, UIControlState.Normal);
				else if (TestPage40251.Arg == "BackgroundImage")
					UIButton.Appearance.SetBackgroundImage(originalValues["BackgroundImage"] as UIImage, UIControlState.Normal);
			}

			base.Dispose(disposing);
		}
	}
}