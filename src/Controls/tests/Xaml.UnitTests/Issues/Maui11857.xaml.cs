using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Xaml.UnitTests
{

	public partial class Maui11857 : ContentPage
	{
		public Maui11857() => InitializeComponent();
		public Maui11857(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Maui11857));

				//shouldn't throw
				var page = new Maui11857(useCompiledXaml);
				Assert.Equal(Colors.HotPink, ((SolidColorBrush)page.label.Background).Color);
			}
		}
	}
}
