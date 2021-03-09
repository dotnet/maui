using System;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh5254VM
	{
		public string Title { get; set; }
		public List<Gh5254VM> Answer { get; set; }
	}

	public partial class Gh5254 : ContentPage
	{
		public Gh5254() => InitializeComponent();
		public Gh5254(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void BindToIntIndexer([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh5254(useCompiledXaml)
				{
					BindingContext = new Gh5254VM
					{
						Answer = new List<Gh5254VM> {
							new Gh5254VM { Title = "Foo"},
							new Gh5254VM { Title = "Bar"},
						}
					}
				};
				Assert.That(layout.label.Text, Is.EqualTo("Foo"));
			}
		}
	}
}
