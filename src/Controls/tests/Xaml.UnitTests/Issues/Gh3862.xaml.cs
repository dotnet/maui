using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh3862 : ContentPage
	{
		public Gh3862()
		{
			InitializeComponent();
		}

		public Gh3862(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(false), TestCase(true)]
			public void OnPlatformMarkupInStyle(bool useCompiledXaml)
			{
				Device.PlatformServices = new MockPlatformServices { RuntimePlatform = Device.iOS };
				var layout = new Gh3862(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Colors.Pink));
				Assert.That(layout.label.IsVisible, Is.False);

				Device.PlatformServices = new MockPlatformServices { RuntimePlatform = Device.Android };

				layout = new Gh3862(useCompiledXaml);
				Assert.That(layout.label.IsVisible, Is.True);

			}
		}
	}
}
