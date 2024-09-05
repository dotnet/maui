using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui23195 : ContentPage
	{
		public Maui23195()
		{
			InitializeComponent();
		}

		public Maui23195(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Test
		{
			[SetUp]
			public void Setup()
			{
				Application.SetCurrentApplication(new MockApplication());
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			}


			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Test]
			public void FontImageColorShouldChangeOnAppThemeChange([Values(false, true)] bool useCompiledXaml)
			{
				var contentPage = new Maui23195(useCompiledXaml);
				Application.Current.MainPage = contentPage;
				Assert.True(contentPage.fontImage.Parent is not null);

				Application.Current.UserAppTheme = AppTheme.Light;
				Assert.AreEqual(contentPage.fontImage.Color, Colors.Black);
				
				Application.Current.UserAppTheme = AppTheme.Dark;
				Assert.AreEqual(contentPage.fontImage.Color, Colors.White);
			}
		}
	}
}