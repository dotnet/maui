using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Bz55343 : ContentPage
	{
		public Bz55343()
		{
			InitializeComponent();
		}

		public Bz55343(bool useCompiledXaml)
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
			public void OnPlatformFontConversion(bool useCompiledXaml)
			{
				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;
				var layout = new Bz55343(useCompiledXaml);
				Assert.That(layout.label0.FontSize, Is.EqualTo(16d));
				Assert.That(layout.label1.FontSize, Is.EqualTo(64d));
			}
		}
	}
}