// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh12763 : ContentPage
	{
		public Gh12763() => InitializeComponent();
		public Gh12763(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void QuotesInStringFormat([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh12763(useCompiledXaml);
				Assert.That(layout.label.Text, Is.EqualTo("\"Foo\""));
			}
		}
	}
}
