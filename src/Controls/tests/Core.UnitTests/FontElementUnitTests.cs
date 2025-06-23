using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public abstract class FontElementUnitTests<TView> : BaseTestFixture
		where TView : IView, new()
	{
		[Theory]
		[InlineData(nameof(IFontElement.FontAttributes), FontAttributes.Bold)]
		[InlineData(nameof(IFontElement.FontAutoScalingEnabled), false)]
		[InlineData(nameof(IFontElement.FontFamily), "Arial")]
		[InlineData(nameof(IFontElement.FontSize), 10)]
		public void FontPropertyTriggersFontProperty(string propertyName, object value)
		{
			var handler = new FontElementHandlerStub();

			var button = new TView();
			button.Handler = handler;
			handler.Updates.Clear();

			button.GetType().GetProperty(propertyName).SetValue(button, value, null);

			Assert.Equal(2, handler.Updates.Count);
			Assert.Equal(new[] { nameof(ITextStyle.Font), propertyName }, handler.Updates);
		}
	}


	public class ButtonFontElementUnitTests : FontElementUnitTests<Button> { }


	public class DatePickerFontElementUnitTests : FontElementUnitTests<DatePicker> { }


	public class EditorFontElementUnitTests : FontElementUnitTests<Editor> { }


	public class EntryFontElementUnitTests : FontElementUnitTests<Entry> { }


	public class LabelFontElementUnitTests : FontElementUnitTests<Label> { }


	public class PickerFontElementUnitTests : FontElementUnitTests<Picker> { }


	public class RadioButtonFontElementUnitTests : FontElementUnitTests<RadioButton> { }


	public class SearchBarFontElementUnitTests : FontElementUnitTests<SearchBar> { }


	public class TimePickerFontElementUnitTests : FontElementUnitTests<TimePicker> { }
}
