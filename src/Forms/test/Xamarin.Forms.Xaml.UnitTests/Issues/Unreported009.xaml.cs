using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Unreported009 : ContentPage
	{
		public Unreported009()
		{
			InitializeComponent();
		}

		public Unreported009(bool useCompiledXaml)
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
				Device.PlatformServices = null;
			}

			[TestCase(true), TestCase(false)]
			public void AllowSetterValueAsElementProperties(bool useCompiledXaml)
			{
				var p = new Unreported009(useCompiledXaml);
				var s = p.Resources["Default"] as Style;
				Assert.AreEqual("Bananas!", (s.Setters[0].Value as Label).Text);
			}
		}
	}
}
