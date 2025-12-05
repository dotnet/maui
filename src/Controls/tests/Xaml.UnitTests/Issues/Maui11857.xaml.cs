using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui11857 : ContentPage
{
	public Maui11857() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void SolidColorBrushAsCompiledResources(XamlInflator inflator)
		{
			//shouldn't throw
			var page = new Maui11857(inflator);
			Assert.Equal(Colors.HotPink, ((SolidColorBrush)page.label.Background).Color);
		}
	}
}
