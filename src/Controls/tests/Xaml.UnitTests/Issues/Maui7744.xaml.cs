using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui7744 : ContentPage
	{
		public Maui7744() => InitializeComponent();
		public Maui7744(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Test
		{
			[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Test]
			public void ConvertersAreExecutedWhileApplyingSetter([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Maui7744(useCompiledXaml);
				Assert.That(page.border0.StrokeShape, Is.TypeOf<RoundRectangle>());
				Assert.That(page.border1.StrokeShape, Is.TypeOf<RoundRectangle>());
			}
		}
	}
}