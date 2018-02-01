using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
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
				Xamarin.Forms.Internals.Registrar.RegisterAll(new Type[0]);
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
