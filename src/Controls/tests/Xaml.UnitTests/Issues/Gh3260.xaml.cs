using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public abstract class Gh3260MyGLayout<T> : Layout<T> where T : View
	{
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			throw new NotImplementedException();
		}
	}

	public class Gh3260MyLayout : Gh3260MyGLayout<View>
	{
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			throw new NotImplementedException();
		}
	}

	public partial class Gh3260 : ContentPage
	{
		public Gh3260()
		{
			InitializeComponent();
		}

		public Gh3260(bool useCompiledXaml)
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
			public void AssignContentWithNoContentAttributeDoesNotThrow(bool useCompiledXaml)
			{
				var layout = new Gh3260(useCompiledXaml);
				Assert.That(layout.mylayout.Children.Count, Is.EqualTo(1));
				Assert.That(layout.mylayout.Children[0], Is.EqualTo(layout.label));
			}
		}
	}
}
