using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue20896 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ContentLayoutPositionsBitmapRightOfTextForPadBehavior()
		{
			var button = new Button
			{
				Text = "IMAGE TEXT",
				ImageSource = ImageSource.FromFile("red.png"),
				HeightRequest = 100,
				WidthRequest = 320,
			};

			await AttachAndRun<ButtonHandler>(button, handler =>
			{
				var nativeButton = handler.PlatformView;
				var bitmap = UIImage.FromBundle("red.png");

				Assert.NotNull(bitmap);

				nativeButton.Configuration = UIButtonConfiguration.BorderedButtonConfiguration;
				nativeButton.PreferredBehavioralStyle = UIBehavioralStyle.Pad;
				nativeButton.SetTitle(button.Text, UIControlState.Normal);
				nativeButton.SetImage(bitmap, UIControlState.Normal);

				button.ContentLayout = new Button.ButtonContentLayout(
					Button.ButtonContentLayout.ImagePosition.Right,
					32);
				handler.UpdateValue(nameof(Button.ContentLayout));

				nativeButton.SetNeedsLayout();
				nativeButton.LayoutIfNeeded();

				var imageFrame = nativeButton.ImageView.Frame;
				var titleFrame = nativeButton.TitleLabel.Frame;
				var imageCenterX = imageFrame.X + (imageFrame.Width / 2);
				var titleCenterX = titleFrame.X + (titleFrame.Width / 2);

				Assert.True(
					imageCenterX > titleCenterX,
					"Button image should be positioned to the right of its text after ContentLayout is updated.");
			});
		}
	}
}
