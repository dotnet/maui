using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Bz51567 : ContentPage
	{
		public Bz51567()
		{
			InitializeComponent();
		}

		public Bz51567(bool useCompiledXaml)
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

			[TestCase(true)]
			[TestCase(false)]
			public void SetterWithElementValue(bool useCompiledXaml)
			{
				var page = new Bz51567(useCompiledXaml);
				var style = page.Resources["ListText"] as Style;
				var setter = style.Setters[1];
				Assert.NotNull(setter);
			}
		}
	}
}
