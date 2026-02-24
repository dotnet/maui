using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui3059rt : ContentPage
{
	public Maui3059rt()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Fact]
		internal void BorderWithMultipleChildren_OnlyLastChildIsUsed()
		{
			var page = new Maui3059rt(XamlInflator.Runtime);
			
			Assert.NotNull(page.Content);
			Assert.IsType<Microsoft.Maui.Controls.Border>(page.Content);
			
			var border = (Microsoft.Maui.Controls.Border)page.Content;
			Assert.NotNull(border.Content);
			Assert.IsType<Label>(border.Content);
			
			var label = (Label)border.Content;
			Assert.Equal("Second", label.Text);
		}

		[Fact]
		internal void MockCompiler_CompileSucceeds()
		{
			MockCompiler.Compile(typeof(Maui3059rt));
		}
	}
}
