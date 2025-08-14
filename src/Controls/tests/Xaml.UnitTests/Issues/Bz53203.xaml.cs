using NUnit.Framework;


namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public enum Bz53203Values
{
	Unknown,
	Good,
	Better,
	Best
}

public partial class Bz53203 : ContentPage
{
	public static int IntValue = 42;
	public static object ObjValue = new object();

	public static readonly BindableProperty ParameterProperty = BindableProperty.CreateAttached("Parameter",
		typeof(object), typeof(Bz53203), null);

	public static object GetParameter(BindableObject obj) =>
		obj.GetValue(ParameterProperty);

	public static void SetParameter(BindableObject obj, object value) =>
		obj.SetValue(ParameterProperty, value);

	public Bz53203()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void MarkupOnAttachedBPDoesNotThrowAtCompileTime([Values(XamlInflator.XamlC)] XamlInflator inflator)
		{
			MockCompiler.Compile(typeof(Bz53203));
		}

		[Test]
		public void MarkupOnAttachedBP([Values] XamlInflator inflator)
		{
			var page = new Bz53203(inflator);
			var label = page.label0;
			Assert.That(Grid.GetRow(label), Is.EqualTo(42));
			Assert.That(GetParameter(label), Is.EqualTo(Bz53203Values.Better));
		}

	}
}