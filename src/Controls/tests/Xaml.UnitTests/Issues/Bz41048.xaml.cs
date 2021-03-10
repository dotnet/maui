using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz41048 : ContentPage
	{
		public Bz41048()
		{
			InitializeComponent();
		}

		public Bz41048(bool useCompiledXaml)
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
				Application.Current = null;
				Device.PlatformServices = null;
			}

			[TestCase(true)]
			[TestCase(false)]
			public void StyleDoesNotOverrideValues(bool useCompiledXaml)
			{
				var layout = new Bz41048(useCompiledXaml);
				var label = layout.label0;
				Assert.That(label.TextColor, Is.EqualTo(Color.Red));
				Assert.That(label.FontAttributes, Is.EqualTo(FontAttributes.Bold));
				Assert.That(label.LineBreakMode, Is.EqualTo(LineBreakMode.WordWrap));
			}
		}
	}
}