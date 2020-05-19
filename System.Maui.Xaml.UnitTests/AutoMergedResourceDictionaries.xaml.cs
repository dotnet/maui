using System;
using NUnit.Framework;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	public partial class AutoMergedResourceDictionaries : ContentPage
	{
		public AutoMergedResourceDictionaries()
		{
			InitializeComponent();
		}

		public AutoMergedResourceDictionaries(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(false)]
			[TestCase(true)]
			public void AutoMergedRd(bool useCompiledXaml)
			{
				var layout = new AutoMergedResourceDictionaries(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.Purple));
				Assert.That(layout.label.BackgroundColor, Is.EqualTo(Color.FromHex("#FF96F3")));
			}
		}
	}
}
