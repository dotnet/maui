using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh2752VM
{
	public Gh2752VM Foo { get; set; }
	public Gh2752VM Bar { get; set; }
	public string Baz { get; set; }
}

public partial class Gh2752 : ContentPage
{
	public static readonly BindableProperty MyProperty =
		BindableProperty.Create(nameof(My), typeof(string), typeof(Gh2752), default(string), defaultValueCreator: b => "default created value");

	public string My
	{
		get { return (string)GetValue(MyProperty); }
		set { SetValue(MyProperty, value); }
	}

	public Gh2752() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void FallbackToDefaultValueCreator([Values] XamlInflator inflator)
		{
			var layout = new Gh2752(inflator) { BindingContext = null };
			Assert.That(layout.My, Is.EqualTo("default created value"));
		}
	}
}