using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui13962CustomCheckBox : CheckBox
{
	public static new readonly BindableProperty IsCheckedProperty =
	BindableProperty.Create(nameof(IsChecked), typeof(bool?), typeof(Maui13962CustomCheckBox), false, BindingMode.TwoWay);

	public new bool? IsChecked
	{
		get { return (bool?)this.GetValue(IsCheckedProperty); }
		set { this.SetValue(IsCheckedProperty, value); }
	}
}

public partial class Maui13962 : ContentView
{

	public Maui13962() => InitializeComponent();

	public Maui13962(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	public class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Method(bool useCompiledXaml)
		{
			//shouln't throw
			var page = new Maui13962(useCompiledXaml);
		}
	}
}