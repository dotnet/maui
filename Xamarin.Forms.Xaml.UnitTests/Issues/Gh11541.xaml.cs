using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh11541 : ContentPage
	{
		public Gh11541() => InitializeComponent();
		public Gh11541(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();

			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void RectangleGeometryDoesntThrow([Values(false, true)] bool useCompiledXaml)
			{
				Assert.DoesNotThrow(()=> new Gh11541(useCompiledXaml));
			}
		}
	}
}
