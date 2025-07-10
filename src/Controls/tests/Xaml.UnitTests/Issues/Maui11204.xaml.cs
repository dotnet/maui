using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui11204 : ContentPage
	{
		public Maui11204() => InitializeComponent();
		public Maui11204(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Test]
			public void VSMSetterOverrideManualValues([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Maui11204(useCompiledXaml);
				Assert.AreEqual(Colors.FloralWhite, page.border.BackgroundColor);
				VisualStateManager.GoToState(page.border, "State1");
				Assert.AreEqual(2, page.border.StrokeThickness);
				Assert.AreEqual(Colors.Blue, page.border.BackgroundColor);
			}

			[Test]
			public void StyleVSMSetterOverrideManualValues([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Maui11204(useCompiledXaml);
				Assert.AreEqual(Colors.HotPink, page.borderWithStyleClass.BackgroundColor);
				VisualStateManager.GoToState(page.borderWithStyleClass, "State1");
				Assert.AreEqual(2, page.borderWithStyleClass.StrokeThickness);
				Assert.AreEqual(Colors.Blue, page.borderWithStyleClass.BackgroundColor);
			}
		}
	}
}