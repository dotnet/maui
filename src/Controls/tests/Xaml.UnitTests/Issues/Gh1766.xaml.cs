using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh1766 : ContentPage
	{
		public Gh1766()
		{
			InitializeComponent();
		}

		public Gh1766(bool useCompiledXaml)
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

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(true), TestCase(false)]
			public void CSSPropertiesNotInerited(bool useCompiledXaml)
			{
				var layout = new Gh1766(useCompiledXaml);
				Assert.That(layout.stack.BackgroundColor, Is.EqualTo(Color.Pink));
				Assert.That(layout.entry.BackgroundColor, Is.EqualTo(VisualElement.BackgroundColorProperty.DefaultValue));
			}
		}
	}
}
