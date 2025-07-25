using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class SafeAreaEdgesTests : ContentPage
	{
		public SafeAreaEdgesTests()
		{
			InitializeComponent();
		}

		public SafeAreaEdgesTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void SingleValueConversions(bool useCompiledXaml)
			{
				var layout = new SafeAreaEdgesTests(useCompiledXaml);

				Assert.AreEqual(SafeAreaEdges.None, layout.singleValueNone.SafeAreaEdges);
				Assert.AreEqual(SafeAreaEdges.All, layout.singleValueAll.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.SoftInput), layout.singleValueSoftInput.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.Container), layout.singleValueContainer.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.Default), layout.singleValueDefault.SafeAreaEdges);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TwoValueConversions(bool useCompiledXaml)
			{
				var layout = new SafeAreaEdgesTests(useCompiledXaml);

				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.SoftInput, SafeAreaRegions.Container), layout.twoValueHorVert.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All), layout.twoValueMixed.SafeAreaEdges);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void FourValueConversions(bool useCompiledXaml)
			{
				var layout = new SafeAreaEdgesTests(useCompiledXaml);

				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.SoftInput, SafeAreaRegions.Container, SafeAreaRegions.All), layout.fourValueMixed.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.Default, SafeAreaRegions.All), layout.fourValueDefault.SafeAreaEdges);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ControlSpecificProperties(bool useCompiledXaml)
			{
				var layout = new SafeAreaEdgesTests(useCompiledXaml);

				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.SoftInput), layout.contentViewTest.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None), layout.contentPageTest.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.SoftInput), layout.borderTest.SafeAreaEdges);
				Assert.AreEqual(new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.Default, SafeAreaRegions.All, SafeAreaRegions.None), layout.scrollViewTest.SafeAreaEdges);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void PropertyInflation_WorksWithAllEnumValues(bool useCompiledXaml)
			{
				var layout = new SafeAreaEdgesTests(useCompiledXaml);

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
}