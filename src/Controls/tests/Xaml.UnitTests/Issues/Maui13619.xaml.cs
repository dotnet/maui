using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui13619 : ContentPage
{
	public Maui13619() => InitializeComponent();

	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());

		[Theory]
		[Values]
		public void AppThemeBindingAndDynamicResource(XamlInflator inflator)
		{
			var page = new Maui13619(inflator);
			Assert.Equal(Colors.HotPink, page.label0.TextColor);
			Assert.Equal(Colors.DarkGray, page.label0.BackgroundColor);

			page.Resources["Primary"] = Colors.SlateGray;
			Assert.Equal(Colors.SlateGray, page.label0.BackgroundColor);

		}
	}
}
