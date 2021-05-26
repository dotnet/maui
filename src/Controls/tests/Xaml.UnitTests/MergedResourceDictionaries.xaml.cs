using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class MergedResourceDictionaries : ContentPage
	{
		public MergedResourceDictionaries()
		{
			InitializeComponent();
		}

		public MergedResourceDictionaries(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
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

			[TestCase(false)]
			[TestCase(true)]
			public void MergedResourcesAreFound(bool useCompiledXaml)
			{
				MockCompiler.Compile(typeof(MergedResourceDictionaries));
				var layout = new MergedResourceDictionaries(useCompiledXaml);
				Assert.That(layout.label0.Text, Is.EqualTo("Foo"));
				Assert.That(layout.label0.TextColor, Is.EqualTo(Colors.Pink));
				Assert.That(layout.label0.BackgroundColor, Is.EqualTo(Color.FromArgb("#111")));
			}
		}
	}
}