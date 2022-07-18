using Microsoft.Maui.Primitives;
using NUnit.Framework;
namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class VisualElementTests
	{
		[Test(Description = "If WidthRequest has been set and is reset to -1, the Core Width should return to being Unset")]
		public void SettingWidthRequestToNegativeOneShouldResetWidth()
		{
			var visualElement = new Label();
			var coreView = visualElement as IView;

			Assert.That(coreView.Width, Is.EqualTo(Dimension.Unset));
			Assert.False(visualElement.IsSet(VisualElement.WidthRequestProperty));

			double testWidth = 100;
			visualElement.WidthRequest = testWidth;

			Assert.That(coreView.Width, Is.EqualTo(testWidth));
			Assert.True(visualElement.IsSet(VisualElement.WidthRequestProperty));
			Assert.That(visualElement.WidthRequest, Is.EqualTo(testWidth));

			// -1 is the legacy "unset" value for WidthRequest; we want to support setting it back to -1 as a way 
			// to "reset" it to the "unset" value.
			visualElement.WidthRequest = -1;

			Assert.That(coreView.Width, Is.EqualTo(Dimension.Unset));
			Assert.That(visualElement.WidthRequest, Is.EqualTo(-1));
		}

		[Test(Description = "If HeightRequest has been set and is reset to -1, the Core Height should return to being Unset")]
		public void SettingHeightRequestToNegativeOneShouldResetWidth()
		{
			var visualElement = new Label();
			var coreView = visualElement as IView;

			Assert.That(coreView.Height, Is.EqualTo(Dimension.Unset));
			Assert.False(visualElement.IsSet(VisualElement.HeightRequestProperty));

			double testHeight = 100;
			visualElement.HeightRequest = testHeight;

			Assert.That(coreView.Height, Is.EqualTo(testHeight));
			Assert.True(visualElement.IsSet(VisualElement.HeightRequestProperty));
			Assert.That(visualElement.HeightRequest, Is.EqualTo(testHeight));

			// -1 is the legacy "unset" value for HeightRequest; we want to support setting it back to -1 as a way 
			// to "reset" it to the "unset" value.
			visualElement.HeightRequest = -1;

			Assert.That(coreView.Height, Is.EqualTo(Dimension.Unset));
			Assert.That(visualElement.HeightRequest, Is.EqualTo(-1));
		}
	}
}
