using Xunit;

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


	public class Tests
	{
		[Fact]
		public void MarkupOnAttachedBPDoesNotThrowAtCompileTime()
		// TODO: Convert to [InlineData] or [MemberData] [Values(XamlInflator.XamlC)] XamlInflator inflator)
		{
			MockCompiler.Compile(typeof(Bz53203));
		}

		[Theory]
		[Values]
		public void MarkupOnAttachedBP(XamlInflator inflator)
		{
			var page = new Bz53203(inflator);
			var label = page.label0;
			Assert.Equal(42, Grid.GetRow(label));
			Assert.Equal(Bz53203Values.Better, GetParameter(label));
		}

	}
}