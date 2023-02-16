using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class SwitchCellTemplateTests : BaseTestFixture
	{
		[Fact]
		public void Create()
		{
			var template = new DataTemplate(typeof(SwitchCell));
			var content = template.CreateContent();

			Assert.IsType<SwitchCell>(content);
		}

		[Fact]
		public void Text()
		{
			var template = new DataTemplate(typeof(SwitchCell));
			template.SetValue(SwitchCell.TextProperty, "text");

			SwitchCell cell = (SwitchCell)template.CreateContent();
			Assert.Equal("text", cell.Text);
		}

		[Fact]
		public void On()
		{
			var template = new DataTemplate(typeof(SwitchCell));
			template.SetValue(SwitchCell.OnProperty, true);

			SwitchCell cell = (SwitchCell)template.CreateContent();
			Assert.True(cell.On);
		}

		[Theory]
		[InlineData(false, true)]
		[InlineData(true, false)]
		public void SwitchCellSwitchChangedArgs(bool initialValue, bool finalValue)
		{
			var template = new DataTemplate(typeof(SwitchCell));
			SwitchCell cell = (SwitchCell)template.CreateContent();

			SwitchCell switchCellFromSender = null;
			bool newSwitchValue = false;

			cell.On = initialValue;

			cell.OnChanged += (s, e) =>
			{
				switchCellFromSender = (SwitchCell)s;
				newSwitchValue = e.Value;
			};

			cell.On = finalValue;

			Assert.Equal(cell, switchCellFromSender);
			Assert.Equal(finalValue, newSwitchValue);
		}
	}
}
