using Microsoft.Maui.Primitives;
using Xunit;
namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class VisualElementTests
	{
		[Fact("If WidthRequest has been set and is reset to -1, the Core Width should return to being Unset")]
		public void SettingWidthRequestToNegativeOneShouldResetWidth()
		{
			var visualElement = new Label();
			var coreView = visualElement as IView;

			Assert.Equal(coreView.Width, Dimension.Unset);
			Assert.False(visualElement.IsSet(VisualElement.WidthRequestProperty));

			double testWidth = 100;
			visualElement.WidthRequest = testWidth;

			Assert.Equal(coreView.Width, testWidth);
			Assert.True(visualElement.IsSet(VisualElement.WidthRequestProperty));
			Assert.Equal(visualElement.WidthRequest, testWidth);

			// -1 is the legacy "unset" value for WidthRequest; we want to support setting it back to -1 as a way 
			// to "reset" it to the "unset" value.
			visualElement.WidthRequest = -1;

			Assert.Equal(coreView.Width, Dimension.Unset);
			Assert.Equal(visualElement.WidthRequest, -1);
		}

		[Fact("If HeightRequest has been set and is reset to -1, the Core Height should return to being Unset")]
		public void SettingHeightRequestToNegativeOneShouldResetWidth()
		{
			var visualElement = new Label();
			var coreView = visualElement as IView;

			Assert.Equal(coreView.Height, Dimension.Unset);
			Assert.False(visualElement.IsSet(VisualElement.HeightRequestProperty));

			double testHeight = 100;
			visualElement.HeightRequest = testHeight;

			Assert.Equal(coreView.Height, testHeight);
			Assert.True(visualElement.IsSet(VisualElement.HeightRequestProperty));
			Assert.Equal(visualElement.HeightRequest, testHeight);

			// -1 is the legacy "unset" value for HeightRequest; we want to support setting it back to -1 as a way 
			// to "reset" it to the "unset" value.
			visualElement.HeightRequest = -1;

			Assert.Equal(coreView.Height, Dimension.Unset);
			Assert.Equal(visualElement.HeightRequest, -1);
		}

		[Fact]
		public void BindingContextPropagatesToBackground()
		{
			var visualElement = new Label();
			var brush = new LinearGradientBrush();
			visualElement.Background = brush;

			var bc1 = new object();
			visualElement.BindingContext = bc1;
			Assert.Equal(bc1, brush.BindingContext);

			var brush2 = new LinearGradientBrush();
			visualElement.Background = brush2;
			Assert.Equal(bc1, brush2.BindingContext);

		}
	}
}
