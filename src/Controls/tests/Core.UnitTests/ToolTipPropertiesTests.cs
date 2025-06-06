using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ToolTipPropertiesTests : BaseTestFixture
	{
		[Fact]
		public void GetText_ReturnsCorrectValue()
		{
			var button = new Button();
			var text = "Test tooltip";

			ToolTipProperties.SetText(button, text);
			var result = ToolTipProperties.GetText(button);

			Assert.Equal(text, result);
		}

		[Fact]
		public void GetDelay_ReturnsCorrectValue()
		{
			var button = new Button();
			var delay = 1000;

			ToolTipProperties.SetDelay(button, delay);
			var result = ToolTipProperties.GetDelay(button);

			Assert.Equal(delay, result);
		}

		[Fact]
		public void GetDuration_ReturnsCorrectValue()
		{
			var button = new Button();
			var duration = 5000;

			ToolTipProperties.SetDuration(button, duration);
			var result = ToolTipProperties.GetDuration(button);

			Assert.Equal(duration, result);
		}

		[Fact]
		public void GetDelay_ReturnsNullWhenNotSet()
		{
			var button = new Button();
			var result = ToolTipProperties.GetDelay(button);

			Assert.Null(result);
		}

		[Fact]
		public void GetDuration_ReturnsNullWhenNotSet()
		{
			var button = new Button();
			var result = ToolTipProperties.GetDuration(button);

			Assert.Null(result);
		}

		[Fact]
		public void GetToolTip_IncludesDelayAndDuration()
		{
			var button = new Button();
			var text = "Test tooltip";
			var delay = 1000;
			var duration = 5000;

			ToolTipProperties.SetText(button, text);
			ToolTipProperties.SetDelay(button, delay);
			ToolTipProperties.SetDuration(button, duration);

			var tooltip = ToolTipProperties.GetToolTip(button);

			Assert.NotNull(tooltip);
			Assert.Equal(text, tooltip.Content);
			Assert.Equal(delay, tooltip.Delay);
			Assert.Equal(duration, tooltip.Duration);
		}

		[Fact]
		public void GetToolTip_ReturnsNullWhenTextNotSet()
		{
			var button = new Button();

			ToolTipProperties.SetDelay(button, 1000);
			ToolTipProperties.SetDuration(button, 5000);

			var tooltip = ToolTipProperties.GetToolTip(button);

			Assert.Null(tooltip);
		}

		[Fact]
		public void GetToolTip_WithNullDelayAndDuration()
		{
			var button = new Button();
			var text = "Test tooltip";

			ToolTipProperties.SetText(button, text);

			var tooltip = ToolTipProperties.GetToolTip(button);

			Assert.NotNull(tooltip);
			Assert.Equal(text, tooltip.Content);
			Assert.Null(tooltip.Delay);
			Assert.Null(tooltip.Duration);
		}
	}
}