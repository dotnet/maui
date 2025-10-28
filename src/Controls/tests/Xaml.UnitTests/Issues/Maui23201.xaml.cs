using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23201
{
	public Maui23201()
	{
		InitializeComponent();
	}

	public Maui23201(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
		{
			Application.Current.Resources.Add("Black", Colors.DarkGray);
			Application.Current.Resources.Add("White", Colors.LightGray);

			Application.Current.UserAppTheme = AppTheme.Light;
			var page = new Maui23201(useCompiledXaml);
			Application.Current.MainPage = page;

			Assert.Equal(Colors.DarkGray, ((FontImageSource)(page.ToolbarItems[0].IconImageSource)).Color);
			Assert.Equal(Colors.Black, ((FontImageSource)(page.ToolbarItems[1].IconImageSource)).Color);

			Application.Current.UserAppTheme = AppTheme.Dark;
			Assert.Equal(Colors.LightGray, ((FontImageSource)(page.ToolbarItems[0].IconImageSource)).Color);
			Assert.Equal(Colors.White, ((FontImageSource)(page.ToolbarItems[1].IconImageSource)).Color);

		}
	}
}