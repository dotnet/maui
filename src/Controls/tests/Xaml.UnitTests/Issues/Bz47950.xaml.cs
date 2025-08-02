using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz47950Behavior : Behavior<View>
{
	public static readonly BindableProperty ColorTestProperty =
		BindableProperty.CreateAttached("ColorTest", typeof(Color), typeof(View), default(Color));

	public static Color GetColorTest(BindableObject bindable) => (Color)bindable.GetValue(ColorTestProperty);
	public static void SetColorTest(BindableObject bindable, Color value) => bindable.SetValue(ColorTestProperty, value);
}

public partial class Bz47950 : ContentPage
{
	public Bz47950()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void BehaviorAndStaticResource([Values] XamlInflator inflator)
		{
			var page = new Bz47950(inflator);
		}
	}
}
