using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Bz49307 : Application
	{
		public Bz49307()
		{
			InitializeComponent();
		}

		public Bz49307(bool useCompiledXaml)
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
			public void ThrowOnMissingDictionary(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<NullReferenceException>(() => new Bz49307(useCompiledXaml));
				else
					Assert.Throws(new XamlParseExceptionConstraint(5, 4), () => new Bz49307(useCompiledXaml));
			}
		}
	}
}