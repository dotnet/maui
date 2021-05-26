using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh13209 : ContentPage
	{
		public Gh13209() => InitializeComponent();
		public Gh13209(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{

			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();

			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[TestCase(true), TestCase(false)]
			public void RdWithSource(bool useCompiledXaml)
			{
				var layout = new Gh13209(useCompiledXaml);
				Assert.That(layout.MyRect.BackgroundColor, Is.EqualTo(Colors.Chartreuse));
				Assert.That(layout.Root.Resources.Count, Is.EqualTo(1));
				Assert.That(layout.Root.Resources.MergedDictionaries.Count, Is.EqualTo(0));

				Assert.That(layout.Root.Resources["Color1"], Is.Not.Null);
				Assert.That(layout.Root.Resources.Remove("Color1"), Is.True);
				Assert.Throws<KeyNotFoundException>(() =>
				{
					var _ = layout.Root.Resources["Color1"];
				});

			}
		}
	}
}
