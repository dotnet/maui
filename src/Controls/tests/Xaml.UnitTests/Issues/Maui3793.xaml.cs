using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui3793 : ContentPage
	{
		public Maui3793() => InitializeComponent();
		public Maui3793(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Tests
		{
			// Constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			// IDisposable public void TearDown() => AppInfo.SetCurrent(null);

			[Theory]
			public void ControlTemplateFromStyle([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
			{
				Maui3793 page;
				Assert.DoesNotThrow(() => page = new Maui3793(useCompiledXaml));
			}
		}
	}
}
