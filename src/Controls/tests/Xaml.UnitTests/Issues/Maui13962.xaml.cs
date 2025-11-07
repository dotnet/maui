using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
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

	public class Test
	{
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());

		[Theory]
		[Values]
		public void ResolutionOfOverridenBP(XamlInflator inflator)
		{
			//shouln't throw
			var page = new Maui13962(inflator);
		}
	}
}