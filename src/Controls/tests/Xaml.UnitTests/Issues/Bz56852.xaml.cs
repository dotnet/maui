using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
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
