using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.Xaml.UnitTests
{

	public partial class Maui11857 : ContentPage
	{
		public Maui11857() => InitializeComponent();
		public Maui11857(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}


		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Test]
			public void SolidColorBrushAsCompiledResources([Values(false, true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Maui11857));

				//shouldn't throw
				var page = new Maui11857(useCompiledXaml);
				Assert.AreEqual(Colors.HotPink, ((SolidColorBrush)page.label.Background).Color);
			}
		}
	}
}
