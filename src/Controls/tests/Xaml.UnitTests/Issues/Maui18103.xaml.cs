using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18103 : ContentPage
{
	public Maui18103() => InitializeComponent();

	public class Test
	{
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());

		[Theory]
		[Values]
		public void VSMOverride(XamlInflator inflator)
		{
			var page = new Maui18103(inflator);
			Assert.Equal(Colors.Orange, ((SolidColorBrush)page.button.Background).Color);
		}
	}
}