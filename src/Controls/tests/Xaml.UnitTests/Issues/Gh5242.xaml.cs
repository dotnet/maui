using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh5242VM
{
	public int? Value { get; set; }
}

public partial class Gh5242 : ContentPage
{
	public static readonly BindableProperty NullableIntProperty = BindableProperty.Create(nameof(NullableInt), typeof(int?), typeof(Gh5242), defaultValue: -1);
	public int? NullableInt { get => (int?)GetValue(NullableIntProperty); }

	public Gh5242() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void BindingToNullable(XamlInflator inflator)
		{
			var layout = new Gh5242(inflator) { BindingContext = new Gh5242VM { Value = 42 } };
			Assert.Equal(42, layout.NullableInt);

			layout.BindingContext = new Gh5242VM { Value = null };
			Assert.Null(layout.NullableInt);
		}
	}
}
