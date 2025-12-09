using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui3793 : ContentPage
{
	public Maui3793() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void ControlTemplateFromStyle(XamlInflator inflator)
		{
			Maui3793 page;
			var ex = Record.Exception(() => page = new Maui3793(inflator));
			Assert.Null(ex);
		}
	}
}
