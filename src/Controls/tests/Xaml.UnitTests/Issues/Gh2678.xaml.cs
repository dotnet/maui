// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh2678 : ContentPage
	{
		public Gh2678() => InitializeComponent();
		public Gh2678(bool useCompiledXaml)
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
				Microsoft.Maui.Controls.Internals.Registrar.RegisterAll(new Type[0]);
			}

			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void StyleClassCanBeChanged([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh2678(useCompiledXaml);
				var label = layout.label0;
				Assert.That(label.BackgroundColor, Is.EqualTo(Color.Red));
				label.StyleClass = new List<string> { "two" };
				Assert.That(label.BackgroundColor, Is.EqualTo(Color.Green));
			}
		}
	}
}
