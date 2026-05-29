using System;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34918 : ContentPage
{
	public Maui34918()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Fact]
		public void ContentPageWithMultipleChildren_FailsXamlCBuild()
		{
			var ex = Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Maui34918)));
			Assert.Equal(0067, ex.Code.CodeCode);
		}

		[Theory]
		[XamlInflatorData]
		internal void ContentPageWithMultipleChildren_LastChildWinsAtRuntime(XamlInflator inflator)
		{
			var page = new Maui34918(inflator);
			Assert.NotNull(page.Content);
			var label = Assert.IsType<Label>(page.Content);
			Assert.Equal("Second", label.Text);
		}
	}
}
