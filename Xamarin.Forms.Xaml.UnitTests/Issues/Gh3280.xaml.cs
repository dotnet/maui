using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh3280 : ContentPage
	{
		public Gh3280()
		{
			InitializeComponent();
		}

		public Size Foo { get; set; }

		public Gh3280(bool useCompiledXaml)
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
			public void SizeHasConverter(bool useCompiledXaml)
			{
				Gh3280 layout = null;
				Assert.DoesNotThrow(() => layout = new Gh3280(useCompiledXaml));
				Assert.That(layout.Foo, Is.EqualTo(new Size(15, 25)));
			}
		}
	}
}
