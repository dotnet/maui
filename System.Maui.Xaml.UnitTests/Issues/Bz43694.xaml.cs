using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Bz43694 : ContentPage
	{
		public Bz43694()
		{
			InitializeComponent();
		}

		public Bz43694(bool useCompiledXaml)
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
			public void xStaticWithOnPlatformChildInRD(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new XamlParseExceptionConstraint(9, 6), () => MockCompiler.Compile(typeof(Bz43694)));
				else
					Assert.Throws(new XamlParseExceptionConstraint(9, 6), () => new Bz43694(useCompiledXaml));
			}
		}
	}
}