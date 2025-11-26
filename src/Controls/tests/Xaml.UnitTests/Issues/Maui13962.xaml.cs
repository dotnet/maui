using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

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

	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void ResolutionOfOverridenBP([Values] XamlInflator inflator)
		{
			//shouln't throw
			var page = new Maui13962(inflator);
		}
	}
}