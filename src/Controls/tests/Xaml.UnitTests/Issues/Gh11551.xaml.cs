using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh11551 : ContentPage
	{
		public Gh11551() => InitializeComponent();
		public Gh11551(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();

			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void RectBoundsDoesntThrow([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh11551(useCompiledXaml);
				var bounds = AbsoluteLayout.GetLayoutBounds(layout.label);
				Assert.That(bounds, Is.EqualTo(new Rect(1, .5, -1, 22)));
			}
		}
	}
}
