using System;
using System.Collections.Generic;

using NUnit.Framework;

using Xamarin.Forms;
using Xamarin.Forms.Build.Tasks;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh4099 : ContentPage
	{
		public Gh4099()
		{
			InitializeComponent();
		}

		public Gh4099(bool useCompiledXaml)
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
			public void BetterExceptionReport(bool useCompiledXaml)
			{
				if (useCompiledXaml)
				{
					try
					{
						MockCompiler.Compile(typeof(Gh4099));
					}
					catch (BuildException xpe)
					{
						Assert.That(xpe.XmlInfo.LineNumber, Is.EqualTo(5));
						Assert.Pass();
					}
					Assert.Fail();
				}
			}
		}
	}
}
