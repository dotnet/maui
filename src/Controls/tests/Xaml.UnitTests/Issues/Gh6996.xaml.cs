// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh6996 : ContentPage
	{
		public Gh6996() => InitializeComponent();
		public Gh6996(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void FontImageSourceColorWithDynamicResource([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh6996(useCompiledXaml);
				Image image = layout.image;
				var fis = image.Source as FontImageSource;
				Assert.That(fis.Color, Is.EqualTo(Color.Orange));

				layout.Resources["imcolor"] = layout.Resources["notBlue"];
				Assert.That(fis.Color, Is.EqualTo(Color.Lime));
			}
		}
	}
}
