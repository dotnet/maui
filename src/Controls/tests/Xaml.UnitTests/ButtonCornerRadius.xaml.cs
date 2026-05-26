using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ButtonCornerRadius : ContentPage
{
	public ButtonCornerRadius() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests() => Application.Current = new MockApplication();
		public void Dispose() => Application.Current = null;

		[Theory]
		[XamlInflatorData]
		internal void EscapedStringsAreTreatedAsLiterals(XamlInflator inflator)
		{
			var layout = new ButtonCornerRadius(inflator);
			Assert.Equal(0, layout.Button0.CornerRadius);
		}
	}
}