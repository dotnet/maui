using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui3059 : ContentPage
{
	public Maui3059()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void BorderWithMultipleChildren_OnlyLastChildIsUsed(XamlInflator inflator)
		{
			var page = new Maui3059(inflator);

			Assert.NotNull(page.Content);
			Assert.IsType<Microsoft.Maui.Controls.Border>(page.Content);

			var border = (Microsoft.Maui.Controls.Border)page.Content;
			Assert.NotNull(border.Content);
			Assert.IsType<Label>(border.Content);

			var label = (Label)border.Content;
			Assert.Equal("Second", label.Text);
		}
	}
}
