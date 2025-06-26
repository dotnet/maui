using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui6367
	{
		public Maui6367() => InitializeComponent();
		public Maui6367(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Test
		{
			// Constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			// IDisposable public void TearDown() => AppInfo.SetCurrent(null);

			[Theory]
			public void NestedTriggers([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
			{
				Maui6367 view = new Maui6367(useCompiledXaml);
			}
		}
	}
}

