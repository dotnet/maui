using UIKit;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using System.Collections.Generic;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls.Issues;

[assembly: ExportRenderer(typeof(Button), typeof(CustomRenderer40251))]
namespace Xamarin.Forms.ControlGallery.iOS
{
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
					Control.TitleShadowOffset = new CoreGraphics.CGSize(2, 2);
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