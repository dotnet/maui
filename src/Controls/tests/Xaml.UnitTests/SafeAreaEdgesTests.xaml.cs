using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

	public partial class SafeAreaEdgesTests : ContentPage
	{
		public SafeAreaEdgesTests() => InitializeComponent();

		[Collection("Xaml Inflation")]
		public class Tests
		{
			[Theory]
			[XamlInflatorData]
			internal void SingleValueConversions(XamlInflator inflator)
			{
				var layout = new SafeAreaEdgesTests(inflator);

				Assert.Equal(SafeAreaEdges.None, layout.singleValueNone.SafeAreaEdges);
				Assert.Equal(SafeAreaEdges.All, layout.singleValueAll.SafeAreaEdges);
				Assert.Equal(new SafeAreaEdges(SafeAreaRegions.SoftInput), layout.singleValueSoftInput.SafeAreaEdges);
				Assert.Equal(new SafeAreaEdges(SafeAreaRegions.Container), layout.singleValueContainer.SafeAreaEdges);
				Assert.Equal(new SafeAreaEdges(SafeAreaRegions.Default), layout.singleValueDefault.SafeAreaEdges);
			}

			[Theory]
			[XamlInflatorData]
			internal void TwoValueConversions(XamlInflator inflator)
			{
				var layout = new SafeAreaEdgesTests(inflator);

				Assert.Equal(new SafeAreaEdges(SafeAreaRegions.SoftInput, SafeAreaRegions.Container), layout.twoValueHorVert.SafeAreaEdges);
				Assert.Equal(new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All), layout.twoValueMixed.SafeAreaEdges);
			}

			[Theory]
			[XamlInflatorData]
			internal void FourValueConversions(XamlInflator inflator)
			{
				var layout = new SafeAreaEdgesTests(inflator);

				Assert.Equal(new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.SoftInput, SafeAreaRegions.Container, SafeAreaRegions.All), layout.fourValueMixed.SafeAreaEdges);
				Assert.Equal(new SafeAreaEdges(SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.Default, SafeAreaRegions.All), layout.fourValueDefault.SafeAreaEdges);
			}

			[Theory]
			[XamlInflatorData]
			internal void ControlSpecificProperties(XamlInflator inflator)
			{
				var layout = new SafeAreaEdgesTests(inflator);

				Assert.Equal(new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.SoftInput), layout.contentViewTest.SafeAreaEdges);
				Assert.Equal(new SafeAreaEdges(SafeAreaRegions.SoftInput), layout.borderTest.SafeAreaEdges);
				Assert.Equal(new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.Default, SafeAreaRegions.All, SafeAreaRegions.None), layout.scrollViewTest.SafeAreaEdges);
			}

			[Theory]
			[XamlInflatorData]
			internal void PropertyInflation_WorksWithAllEnumValues(XamlInflator inflator)
			{
				var layout = new SafeAreaEdgesTests(inflator);

				// Verify all enum values are properly inflated from XAML strings
				var noneGrid = layout.singleValueNone;
				var allGrid = layout.singleValueAll;
				var softInputGrid = layout.singleValueSoftInput;
				var containerGrid = layout.singleValueContainer;
				var defaultGrid = layout.singleValueDefault;

				Assert.Equal(SafeAreaRegions.None, noneGrid.SafeAreaEdges.Left);
				Assert.Equal(SafeAreaRegions.All, allGrid.SafeAreaEdges.Left);
				Assert.Equal(SafeAreaRegions.SoftInput, softInputGrid.SafeAreaEdges.Left);
				Assert.Equal(SafeAreaRegions.Container, containerGrid.SafeAreaEdges.Left);
				Assert.Equal(SafeAreaRegions.Default, defaultGrid.SafeAreaEdges.Left);
			}
		}
	}
