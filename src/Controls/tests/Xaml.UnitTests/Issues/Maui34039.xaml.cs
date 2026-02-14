using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34039 : ContentPage
{
	public Maui34039() => InitializeComponent();

	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void SetterWithPropertyElementValueInTriggerIsAdded(XamlInflator inflator)
		{
			var page = new Maui34039(inflator);
			Assert.NotNull(page);
			// Verify the trigger has a setter with a FontImageSource value
			var style = page.button.Style;
			Assert.NotNull(style);
			Assert.Single(style.Triggers);
			var trigger = (Trigger)style.Triggers[0];
			Assert.Single(trigger.Setters);
			Assert.IsType<FontImageSource>(trigger.Setters[0].Value);
		}
	}
}
