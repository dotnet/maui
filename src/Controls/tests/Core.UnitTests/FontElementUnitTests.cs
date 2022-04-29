using Microsoft.Maui.Controls.Internals;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public abstract class FontElementUnitTests<TView> : BaseTestFixture
		where TView : IView, new()
	{
		[Test]
		[TestCase(nameof(IFontElement.FontAttributes), FontAttributes.Bold)]
		[TestCase(nameof(IFontElement.FontAutoScalingEnabled), false)]
		[TestCase(nameof(IFontElement.FontFamily), "Arial")]
		[TestCase(nameof(IFontElement.FontSize), 10)]
		public void FontPropertyTriggersFontProperty(string propertyName, object value)
		{
			var handler = new FontElementHandlerStub();

			var button = new TView();
			button.Handler = handler;
			handler.Updates.Clear();

			button.GetType().GetProperty(propertyName).SetValue(button, value, null);

			Assert.AreEqual(2, handler.Updates.Count);
			Assert.AreEqual(new[] { propertyName, nameof(ITextStyle.Font) }, handler.Updates);
		}
	}

	[TestFixture]
	public class ButtonFontElementUnitTests : FontElementUnitTests<Button> { }

	[TestFixture]
	public class DatePickerFontElementUnitTests : FontElementUnitTests<DatePicker> { }

	[TestFixture]
	public class EditorFontElementUnitTests : FontElementUnitTests<Editor> { }

	[TestFixture]
	public class EntryFontElementUnitTests : FontElementUnitTests<Entry> { }

	[TestFixture]
	public class LabelFontElementUnitTests : FontElementUnitTests<Label> { }

	[TestFixture]
	public class PickerFontElementUnitTests : FontElementUnitTests<Picker> { }

	[TestFixture]
	public class RadioButtonFontElementUnitTests : FontElementUnitTests<RadioButton> { }

	[TestFixture]
	public class SearchBarFontElementUnitTests : FontElementUnitTests<SearchBar> { }

	[TestFixture]
	public class TimePickerFontElementUnitTests : FontElementUnitTests<TimePicker> { }
}
