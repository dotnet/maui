using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui22105
	{
		public Maui22105()
		{
			InitializeComponent();
		}

		public Maui22105(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Test
		{
			// Constructor
			public void Setup()
			{
				Application.SetCurrentApplication(new MockApplication());
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			}

			// IDisposable public void TearDown() => AppInfo.SetCurrent(null);

			[Theory]
			public void DefaultValueShouldBeApplied([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
			{
				var page = new Maui22105(useCompiledXaml);
				Assert.Equal(100, page.label.FontSize);
			}
		}
	}
}