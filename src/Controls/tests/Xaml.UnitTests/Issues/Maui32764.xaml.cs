// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Simulate third-party control pattern (like CardView.MAUI) where constructors use interface parameters
public interface ITestProcessor
{
}

public class TestProcessor : ITestProcessor
{
	public double ScaleFactor { get; set; }
}

public class TestCardsView : ContentView
{
	public TestCardsView(ITestProcessor processor)
	{
		// Constructor that takes interface parameter
	}
}

public class TestCarouselView : TestCardsView
{
	// Parameterless constructor chains to constructor with interface parameter
	public TestCarouselView() : this(new TestProcessor())
	{
	}

	// Constructor with interface parameter that chains to base
	public TestCarouselView(ITestProcessor processor) : base(processor)
	{
	}
}

public partial class Maui32764 : ContentPage
{
	public Maui32764() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void XArgumentsWithInterfaceParameterShouldWork([Values] XamlInflator inflator)
		{
			// This test reproduces issue #32764
			// The bug was that MatchXArguments incorrectly checked class inheritance
			// before interface implementation, causing it to fail to match constructors
			// with interface parameters when using x:Arguments in XAML
			var page = new Maui32764(inflator);
			
			// Just verifying the page loads without throwing is sufficient
			// The original bug would throw MAUIX2003: "No method found for 'TestCarouselView'"
			// because the source generator couldn't match the constructor taking ITestProcessor
			Assert.That(page, Is.Not.Null);
			Assert.That(page.carouselView, Is.Not.Null);
		}
	}
}
