using System;
using System.Collections.Generic;

using Xamarin.Forms;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh2007 : ContentPage
	{
		public Gh2007()
		{
			InitializeComponent();
		}

		public Gh2007(bool useCompiledXaml)
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

			[TestCase(false), TestCase(true)]
			public void UsefullxResourceErrorMessages(bool useCompiledXaml)
			{
				Assert.Throws<XamlParseException>(() => new Gh2007(useCompiledXaml));
			}
		}
	}
}
