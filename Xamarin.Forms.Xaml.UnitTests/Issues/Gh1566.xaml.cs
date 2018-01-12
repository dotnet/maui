using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh1566
	{
		public Gh1566()
		{
			InitializeComponent();
		}

		public Gh1566(bool useCompiledXaml)
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
			public void ObsoletePropsDoNotThrow(bool useCompiledXaml)
			{
				var layout = new Gh1566(useCompiledXaml);
				Assert.That(layout.frame.BorderColor, Is.EqualTo(Color.Red));
			}
		}
	}
}
