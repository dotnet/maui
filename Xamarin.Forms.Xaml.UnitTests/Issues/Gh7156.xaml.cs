using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh7156 : ContentPage
	{
		public Gh7156() => InitializeComponent();
		public Gh7156(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void OnPlatformDefaultToBPDefaultValue([Values(true, false)]bool useCompiledXaml)
			{
				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;
				var layout = new Gh7156(useCompiledXaml);
				Assert.That(layout.l0.Text, Is.EqualTo(Label.TextProperty.DefaultValue));
				Assert.That(layout.l0.WidthRequest, Is.EqualTo(VisualElement.WidthRequestProperty.DefaultValue));
				Assert.That(layout.l1.Text, Is.EqualTo("bar"));
				Assert.That(layout.l1.WidthRequest, Is.EqualTo(20d));
			}
		}
	}
}
