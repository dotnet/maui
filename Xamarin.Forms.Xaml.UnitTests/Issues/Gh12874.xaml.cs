using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh12874 : ContentPage
	{
		public Gh12874() => InitializeComponent();
		public Gh12874(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void RevertToStyleValue([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh12874(useCompiledXaml);
				Assert.That(layout.label0.HorizontalOptions, Is.EqualTo(LayoutOptions.Start));
				Assert.That(layout.label1.HorizontalOptions, Is.EqualTo(LayoutOptions.Start));
				layout.label0.ClearValue(Label.HorizontalOptionsProperty);
				layout.label1.ClearValue(Label.HorizontalOptionsProperty);
				Assert.That(layout.label0.HorizontalOptions, Is.EqualTo(LayoutOptions.Center));
				Assert.That(layout.label1.HorizontalOptions, Is.EqualTo(LayoutOptions.Center));
			}
		}
	}
}
