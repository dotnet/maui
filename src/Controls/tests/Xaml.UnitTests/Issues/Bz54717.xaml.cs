using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz54717 : ContentPage
	{
		public Bz54717()
		{
			InitializeComponent();
		}

		public Bz54717(bool useCompiledXaml)
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
			public void Foo(bool useCompiledXaml)
			{
				Application.Current = new MockApplication
				{
					Resources = new ResourceDictionary {
						{"Color1", Colors.Red},
						{"Color2", Colors.Blue},
					}
				};
				var layout = new Bz54717(useCompiledXaml);
				Assert.That(layout.Resources.Count, Is.EqualTo(1));
				var array = layout.Resources["SomeColors"] as Color[];
				Assert.That(array[0], Is.EqualTo(Colors.Red));
				Assert.That(array[1], Is.EqualTo(Colors.Blue));
			}
		}
	}
}
