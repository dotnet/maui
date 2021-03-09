using System;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh11620 : ContentPage
	{
		public Gh11620() => InitializeComponent();
		public Gh11620(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void BoxOnAdd([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh11620(useCompiledXaml);
				var arr = layout.Resources["myArray"];
				Assert.That(arr, Is.TypeOf<object[]>());
				Assert.That(((object[])arr).Length, Is.EqualTo(3));
				Assert.That(((object[])arr)[2], Is.EqualTo(32));
			}
		}
	}
}