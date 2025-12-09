using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui7744 : ContentPage
{
	public Maui7744() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void ConvertersAreExecutedWhileApplyingSetter(XamlInflator inflator)
		{
			var page = new Maui7744(inflator);
			Assert.IsType<RoundRectangle>(page.border0.StrokeShape);
			Assert.IsType<RoundRectangle>(page.border1.StrokeShape);
		}
	}
}