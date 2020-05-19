using System;
using NUnit.Framework;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	public partial class Bz56852
	{
		public Bz56852()
		{
			InitializeComponent();
		}

		public Bz56852(bool useCompiledXaml)
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
				Application.Current = null;
			}

			[TestCase(true)]
			[TestCase(false)]
			public void DynamicResourceApplyingOrder(bool useCompiledXaml)
			{
				var layout = new Bz56852(useCompiledXaml);
				Assert.That(layout.label.FontSize, Is.EqualTo(50));
			}
		}
	}
}
