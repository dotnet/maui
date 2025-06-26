using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui7744 : ContentPage
{
	public Maui7744() => InitializeComponent();
	public Maui7744(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	class Test
	{
		// Constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		// IDisposable public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
		public void ConvertersAreExecutedWhileApplyingSetter([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
		{
			var page = new Maui7744(useCompiledXaml);
			Assert.That(page.border0.StrokeShape, Is.TypeOf<RoundRectangle>());
			Assert.That(page.border1.StrokeShape, Is.TypeOf<RoundRectangle>());
		}
	}
}