using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18980 : ContentPage
{
	public Maui18980() => InitializeComponent();


	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void CSSnotOverridenbyImplicitStyle(XamlInflator inflator)
		{
			// var app = new MockApplication();
			// app.Resources.Add(new Maui18980Style(inflator));
			// Application.SetCurrentApplication(app);

			var page = new Maui18980(inflator);
			Assert.Equal(Colors.Red, page.button.BackgroundColor);
		}
	}
}
