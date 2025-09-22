using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

	public partial class SafeAreaEdgesTests : ContentPage
	{
		public SafeAreaEdgesTests() => InitializeComponent();

		[TestFixture]
		class Tests
		{
			[Test]
			public void SingleValueConversions([Values] XamlInflator inflator)
			{
				var layout = new SafeAreaEdgesTests(inflator);

				Assert.AreEqual(SafeAreaEdges.None, layout.singleValueNone.SafeAreaEdges);
				Assert.AreEqual(SafeAreaEdges.All, layout.singleValueAll.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.SoftInput), layout.singleValueSoftInput.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.Container), layout.singleValueContainer.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.Default), layout.singleValueDefault.SafeAreaEdges);
			}

			[Test]
			public void TwoValueConversions([Values] XamlInflator inflator)
			{
				var layout = new SafeAreaEdgesTests(inflator);

				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.SoftInput, SafeAreaRegions.Container), layout.twoValueHorVert.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All), layout.twoValueMixed.SafeAreaEdges);
			}

			[Test]
			public void FourValueConversions([Values] XamlInflator inflator)
			{
				var layout = new SafeAreaEdgesTests(inflator);

				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.SoftInput, SafeAreaRegions.Container, SafeAreaRegions.All), layout.fourValueMixed.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.Default, SafeAreaRegions.All), layout.fourValueDefault.SafeAreaEdges);
			}

			[Test]
			public void ControlSpecificProperties([Values] XamlInflator inflator)
			{
				var layout = new SafeAreaEdgesTests(inflator);

				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.SoftInput), layout.contentViewTest.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.SoftInput), layout.borderTest.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.Default, SafeAreaRegions.All, SafeAreaRegions.None), layout.scrollViewTest.SafeAreaEdges);
			}

			[Test]
			public void PropertyInflation_WorksWithAllEnumValues([Values] XamlInflator inflator)
			{
				var layout = new SafeAreaEdgesTests(inflator);

				// Verify all enum values are properly inflated from XAML strings
				var noneGrid = layout.singleValueNone;
				var allGrid = layout.singleValueAll;
				var softInputGrid = layout.singleValueSoftInput;
				var containerGrid = layout.singleValueContainer;
				var defaultGrid = layout.singleValueDefault;

				Assert.AreEqual(SafeAreaRegions.None, noneGrid.SafeAreaEdges.Left);
				Assert.AreEqual(SafeAreaRegions.All, allGrid.SafeAreaEdges.Left);
				Assert.AreEqual(SafeAreaRegions.SoftInput, softInputGrid.SafeAreaEdges.Left);
				Assert.AreEqual(SafeAreaRegions.Container, containerGrid.SafeAreaEdges.Left);
				Assert.AreEqual(SafeAreaRegions.Default, defaultGrid.SafeAreaEdges.Left);
			}
		}
	}
