using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Gh2130Behavior
{
	public static readonly BindableProperty AppearingProperty =
		BindableProperty.Create("Appearing", typeof(bool), typeof(Gh2130Behavior), default(bool));

	public static bool GetAppearing(BindableObject bindable)
	{
		return (bool)bindable.GetValue(AppearingProperty);
	}

	public static void SetAppearing(BindableObject bindable, bool value)
	{
		bindable.SetValue(AppearingProperty, value);
	}

}

public partial class Gh2130 : ContentPage
{
	public Gh2130() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void AttachedBPWithEventName(XamlInflator inflator)
		{
			new Gh2130(inflator);
		}
	}
}
