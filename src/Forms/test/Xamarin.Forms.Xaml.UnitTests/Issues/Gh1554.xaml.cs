using System;
using System.Linq;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh1554
	{
		public Gh1554()
		{
			InitializeComponent();
		}

		public Gh1554(bool useCompiledXaml)
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
			public void NestedRDAreOnlyProcessedOnce(bool useCompiledXaml)
			{
				var layout = new Gh1554(useCompiledXaml);
				Assert.That(layout.Resources.MergedDictionaries.First().First().Key, Is.EqualTo("label0"));
			}
		}
	}
}
