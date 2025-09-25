using System;
using Microsoft.Maui.Converters;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class SafeAreaTests : BaseTestFixture
	{
		[Fact]
		public void GetEdges_DefaultValue_ReturnsDefault()
		{
			var layout = new Grid();

			var result = layout.SafeAreaEdges;

			Assert.Equal(SafeAreaEdges.Container, result);
		}

		[Fact]
		public void SetEdges_SingleValue_AppliedCorrectly()
		{
			var layout = new Grid();
			var value = SafeAreaEdges.All;

			layout.SafeAreaEdges = value;
			var result = layout.SafeAreaEdges;

			Assert.Equal(value, result);
		}

		[Fact]
		public void GetEdgeValue_UniformValue_AppliesAllEdges()
		{
			var layout = new Grid();
			layout.SafeAreaEdges = SafeAreaEdges.All;

			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetEdgeValue(layout, 0)); // Left
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetEdgeValue(layout, 1)); // Top
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetEdgeValue(layout, 2)); // Right
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetEdgeValue(layout, 3)); // Bottom
		}

		[Fact]
		public void GetEdgeValue_TwoValues_AppliesCorrectly()
		{
			var layout = new Grid();
			layout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None);

			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetEdgeValue(layout, 0)); // Left
			Assert.Equal(SafeAreaRegions.None, SafeAreaElement.GetEdgeValue(layout, 1)); // Top
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetEdgeValue(layout, 2)); // Right
			Assert.Equal(SafeAreaRegions.None, SafeAreaElement.GetEdgeValue(layout, 3)); // Bottom
		}

		[Fact]
		public void GetEdgeValue_FourValues_AppliesCorrectly()
		{
			var layout = new Grid();
			layout.SafeAreaEdges = new SafeAreaEdges(
				SafeAreaRegions.All,   // Left
				SafeAreaRegions.None,  // Top
				SafeAreaRegions.All,   // Right
				SafeAreaRegions.None   // Bottom
			);

			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetEdgeValue(layout, 0)); // Left
			Assert.Equal(SafeAreaRegions.None, SafeAreaElement.GetEdgeValue(layout, 1)); // Top
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetEdgeValue(layout, 2)); // Right
			Assert.Equal(SafeAreaRegions.None, SafeAreaElement.GetEdgeValue(layout, 3)); // Bottom
		}

		[Fact]
		public void GetEdgeValue_InvalidEdgeIndex_ReturnsNone()
		{
			var layout = new Grid();
			layout.SafeAreaEdges = SafeAreaEdges.All;

			Assert.Equal(SafeAreaRegions.None, SafeAreaElement.GetEdgeValue(layout, -1));
			Assert.Equal(SafeAreaRegions.None, SafeAreaElement.GetEdgeValue(layout, 4));
		}

		[Fact]
		public void SafeAreaEdges_DefaultConstructor_ReturnsNone()
		{
			var edges = new SafeAreaEdges();

			Assert.Equal(SafeAreaRegions.None, edges.Left);
			Assert.Equal(SafeAreaRegions.None, edges.Top);
			Assert.Equal(SafeAreaRegions.None, edges.Right);
			Assert.Equal(SafeAreaRegions.None, edges.Bottom);
		}

		[Fact]
		public void SafeAreaEdges_UniformConstructor_AppliesAllEdges()
		{
			var edges = new SafeAreaEdges(SafeAreaRegions.All);

			Assert.Equal(SafeAreaRegions.All, edges.Left);
			Assert.Equal(SafeAreaRegions.All, edges.Top);
			Assert.Equal(SafeAreaRegions.All, edges.Right);
			Assert.Equal(SafeAreaRegions.All, edges.Bottom);
		}

		[Fact]
		public void SafeAreaEdges_HorizontalVerticalConstructor_AppliesCorrectly()
		{
			var edges = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None);

			Assert.Equal(SafeAreaRegions.All, edges.Left);   // horizontal
			Assert.Equal(SafeAreaRegions.None, edges.Top);   // vertical
			Assert.Equal(SafeAreaRegions.All, edges.Right);  // horizontal
			Assert.Equal(SafeAreaRegions.None, edges.Bottom); // vertical
		}

		[Fact]
		public void SafeAreaEdges_IndividualConstructor_AppliesCorrectly()
		{
			var edges = new SafeAreaEdges(
				SafeAreaRegions.All,   // Left
				SafeAreaRegions.None,  // Top
				SafeAreaRegions.All,   // Right
				SafeAreaRegions.None   // Bottom
			);

			Assert.Equal(SafeAreaRegions.All, edges.Left);
			Assert.Equal(SafeAreaRegions.None, edges.Top);
			Assert.Equal(SafeAreaRegions.All, edges.Right);
			Assert.Equal(SafeAreaRegions.None, edges.Bottom);
		}

		[Fact]
		public void SafeAreaEdges_GetEdge_ReturnsCorrectValues()
		{
			var edges = new SafeAreaEdges(
				SafeAreaRegions.All,   // Left
				SafeAreaRegions.None,  // Top
				SafeAreaRegions.All,   // Right
				SafeAreaRegions.None   // Bottom
			);

			Assert.Equal(SafeAreaRegions.All, edges.GetEdge(0));   // Left
			Assert.Equal(SafeAreaRegions.None, edges.GetEdge(1));  // Top
			Assert.Equal(SafeAreaRegions.All, edges.GetEdge(2));   // Right
			Assert.Equal(SafeAreaRegions.None, edges.GetEdge(3));  // Bottom
			Assert.Equal(SafeAreaRegions.None, edges.GetEdge(-1)); // Invalid
			Assert.Equal(SafeAreaRegions.None, edges.GetEdge(4));  // Invalid
		}

		[Fact]
		public void SafeAreaEdges_StaticProperties_ReturnCorrectValues()
		{
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.None), SafeAreaEdges.None);
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.All), SafeAreaEdges.All);
		}

		[Fact]
		public void SafeAreaEdges_Equality_WorksCorrectly()
		{
			var edges1 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None);
			var edges2 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None);
			var edges3 = new SafeAreaEdges(SafeAreaRegions.None);

			Assert.True(edges1 == edges2);
			Assert.False(edges1 == edges3);
			Assert.True(edges1 != edges3);
			Assert.True(edges1.Equals(edges2));
			Assert.False(edges1.Equals(edges3));
		}

		[Fact]
		public void SafeAreaEdges_ToString_FormatsCorrectly()
		{
			var uniform = new SafeAreaEdges(SafeAreaRegions.All);
			var twoValue = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None);
			var fourValue = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None);
			var asymmetric = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.All);

			Assert.Equal("All, All, All, All", uniform.ToString());
			Assert.Equal("All, None, All, None", twoValue.ToString());
			Assert.Equal("All, None, All, None", fourValue.ToString()); // Optimized: Left==Right, Top==Bottom
			Assert.Equal("All, None, None, All", asymmetric.ToString()); // No optimization possible
		}

		[Fact]
		public void Layout_ImplementsISafeAreaView()
		{
			var layout = new Grid();

			Assert.IsAssignableFrom<ISafeAreaView>(layout);
			Assert.IsAssignableFrom<ISafeAreaView2>(layout);
		}

		[Fact]
		public void Layout_GetSafeAreaRegionsForEdge_UsesDirectProperty()
		{
			var layout = new Grid();
			layout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All);

			// Test via ISafeAreaView2 interface - direct region values
			var safeAreaView2 = (ISafeAreaView2)layout;

			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(0)); // Left = None
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(1));  // Top = All
			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(2)); // Right = None
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(3));  // Bottom = All
		}

		[Fact]
		public void Layout_GetSafeAreaRegionsForEdge_FallsBackToLegacyProperty()
		{
			var layout = new Grid();
			layout.IgnoreSafeArea = true; // Legacy property

			// Should fall back to legacy property when no direct property is set
			// IgnoreSafeArea = true should result in SafeAreaRegions.None (edge-to-edge)
			var safeAreaView2 = (ISafeAreaView2)layout;

			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(0));
			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(1));
			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(2));
			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(3));
		}

		[Fact]
		public void Page_ImplementsISafeAreaView()
		{
			var page = new ContentPage();

			Assert.IsAssignableFrom<ISafeAreaView>(page);
			Assert.IsAssignableFrom<ISafeAreaView2>(page);
		}

		[Fact]
		public void ContentView_ImplementsISafeAreaView()
		{
			var contentView = new ContentView();

			Assert.IsAssignableFrom<ISafeAreaView2>(contentView);
		}

		[Fact]
		public void Page_GetSafeAreaRegionsForEdge_UsesDirectProperty()
		{
			var page = new ContentPage();
			page.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All);

			// Test via ISafeAreaView2 interface - direct region values
			var safeAreaView2 = (ISafeAreaView2)page;

			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(0)); // Left = None
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(1));  // Top = All
			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(2)); // Right = None
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(3));  // Bottom = All
		}

		[Fact]
		public void ContentView_GetSafeAreaRegionsForEdge_DefaultsToNoneWhenNoPropertySet()
		{
			var contentView = new ContentView(); // ContentView implements ISafeAreaView2

			// Should default to SafeAreaRegions.None when no property is set (edge-to-edge behavior)
			var safeAreaView2 = (ISafeAreaView2)contentView;

			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(0));
			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(1));
			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(2));
			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(3));
		}

		[Fact]
		public void Page_GetSafeAreaRegionsForEdge_DefaultsToNoneForContentPage()
		{
			var page = new ContentPage(); // ContentPage defaults to SafeAreaRegions.None on iOS (IgnoreSafeArea = true)

			// ContentPage has special logic - defaults to SafeAreaRegions.None (edge-to-edge) on iOS
			var safeAreaView2 = (ISafeAreaView2)page;

			// only iOS defaults to "None" for ContentPage so we are just validating that the default is container
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(0));
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(1));
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(2));
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(3));
		}

		// Tests based on existing iOS safe area usage patterns
		[Fact]
		public void SafeArea_CanReplaceUseSafeAreaScenario()
		{
			// This test mimics the usage pattern from Issue3809 and ShellTests.iOS
			var contentView = new ContentView()
			{
				Content = new Label() { Text = "Test Page" }
			};

			// Legacy approach: contentPage.On<iOS>().SetUseSafeArea(true);
			// New approach: use SafeAreaEdges property to obey all safe areas
			contentView.SafeAreaEdges = SafeAreaEdges.All; // Obey all safe areas

			var safeAreaView2 = (ISafeAreaView2)contentView;

			// All edges should obey safe area (return All region)
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(0));
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(1));
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(2));
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(3));
		}

		[Fact]
		public void SafeArea_SupportsPerEdgeControl_IgnoreTopAndBottom()
		{
			// Common use case: ignore safe area for top and bottom edges only (e.g., for full-width background)
			var stackLayout = new VerticalStackLayout();

			// Edge-to-edge for top and bottom (None), obey for left and right (All)
			stackLayout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None);

			var safeAreaView2 = (ISafeAreaView2)stackLayout;

			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(0));  // Left = All
			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(1)); // Top = None
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(2));  // Right = All
			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(3)); // Bottom = None
		}

		[Fact]
		public void SafeArea_TwoValueShorthand_LeftRightTopBottom()
		{
			// Test two-value shorthand: first value for left/right, second for top/bottom
			var grid = new Grid();

			grid.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All);

			var safeAreaView2 = (ISafeAreaView2)grid;

			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(0)); // Left = None
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(1));  // Top = All
			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(2)); // Right = None
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(3));  // Bottom = All
		}

		[Fact]
		public void SafeAreaEdgesTypeConverter_CanConvertFromString()
		{
			var converter = new SafeAreaEdgesTypeConverter();

			Assert.True(converter.CanConvertFrom(null, typeof(string)));
			Assert.False(converter.CanConvertFrom(null, typeof(int)));
		}

		[Fact]
		public void SafeAreaEdgesTypeConverter_ConvertFromSingleValue()
		{
			var converter = new SafeAreaEdgesTypeConverter();

			var result = (SafeAreaEdges)converter.ConvertFrom(null, null, "All");

			Assert.Equal(SafeAreaEdges.All, result);
		}

		[Fact]
		public void SafeAreaEdgesTypeConverter_ConvertFromTwoValues()
		{
			var converter = new SafeAreaEdgesTypeConverter();

			var result = (SafeAreaEdges)converter.ConvertFrom(null, null, "None,Container");

			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.Container), result);
		}

		[Fact]
		public void SafeAreaEdgesTypeConverter_ConvertFromFourValues()
		{
			var converter = new SafeAreaEdgesTypeConverter();

			var result = (SafeAreaEdges)converter.ConvertFrom(null, null, "None,All,Container,SoftInput");

			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.Container, SafeAreaRegions.SoftInput), result);
		}

		[Fact]
		public void SafeAreaEdgesTypeConverter_ConvertFromInvalidValue_ThrowsException()
		{
			var converter = new SafeAreaEdgesTypeConverter();

			Assert.Throws<FormatException>(() =>
				converter.ConvertFrom(null, null, "InvalidValue"));
		}

		[Fact]
		public void SafeAreaEdgesTypeConverter_ConvertFromInvalidLength_ThrowsException()
		{
			var converter = new SafeAreaEdgesTypeConverter();

			Assert.Throws<FormatException>(() =>
				converter.ConvertFrom(null, null, "None,Container,All")); // 3 values not supported
		}

		[Fact]
		public void SafeAreaEdgesTypeConverter_ConvertToString()
		{
			var converter = new SafeAreaEdgesTypeConverter();
			var edges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.Container);

			var result = converter.ConvertTo(null, null, edges, typeof(string));

			Assert.Equal("None, Container, None, Container", result);
		}

		[Fact]
		public void SafeAreaEdges_AllEnumValues_WorkCorrectly()
		{
			// Test all individual enum values can be used
			var noneEdges = new SafeAreaEdges(SafeAreaRegions.None);
			var softInputEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput);
			var containerEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			var defaultEdges = new SafeAreaEdges(SafeAreaRegions.Default);
			var allEdges = new SafeAreaEdges(SafeAreaRegions.All);

			Assert.Equal(SafeAreaRegions.None, noneEdges.Left);
			Assert.Equal(SafeAreaRegions.SoftInput, softInputEdges.Left);
			Assert.Equal(SafeAreaRegions.Container, containerEdges.Left);
			Assert.Equal(SafeAreaRegions.Default, defaultEdges.Left);
			Assert.Equal(SafeAreaRegions.All, allEdges.Left);
		}

		[Fact]
		public void SafeAreaEdgesTypeConverter_AllEnumValues_ConvertCorrectly()
		{
			var converter = new SafeAreaEdgesTypeConverter();

			// Test all individual enum values can be parsed
			var noneResult = (SafeAreaEdges)converter.ConvertFrom(null, null, "None");
			var softInputResult = (SafeAreaEdges)converter.ConvertFrom(null, null, "SoftInput");
			var containerResult = (SafeAreaEdges)converter.ConvertFrom(null, null, "Container");
			var defaultResult = (SafeAreaEdges)converter.ConvertFrom(null, null, "Default");
			var allResult = (SafeAreaEdges)converter.ConvertFrom(null, null, "All");

			Assert.Equal(SafeAreaEdges.None, noneResult);
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.SoftInput), softInputResult);
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.Container), containerResult);
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.Default), defaultResult);
			Assert.Equal(SafeAreaEdges.All, allResult);
		}

		[Fact]
		public void SafeAreaEdgesTypeConverter_MixedEnumValues_ConvertCorrectly()
		{
			var converter = new SafeAreaEdgesTypeConverter();

			// Test mixed combinations of all enum values
			var mixedTwoValues = (SafeAreaEdges)converter.ConvertFrom(null, null, "SoftInput,Container");
			var mixedFourValues = (SafeAreaEdges)converter.ConvertFrom(null, null, "SoftInput,Default,Container,All");

			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.SoftInput, SafeAreaRegions.Container), mixedTwoValues);
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.SoftInput, SafeAreaRegions.Default, SafeAreaRegions.Container, SafeAreaRegions.All), mixedFourValues);
		}

		[Fact]
		public void SafeAreaEdges_EnumBitValues_AreUnique()
		{
			// Verify enum values use unique bit patterns
			Assert.Equal(0, (int)SafeAreaRegions.None);
			Assert.Equal(1, (int)SafeAreaRegions.SoftInput);
			Assert.Equal(2, (int)SafeAreaRegions.Container);
			Assert.Equal(-1, (int)SafeAreaRegions.Default);
			Assert.Equal(32768, (int)SafeAreaRegions.All);
		}

		[Fact]
		public void SafeAreaEdges_TypeConverterWithBinding_SimulatedConversion()
		{
			// Test that the type converter works as it would in a binding scenario
			var converter = new SafeAreaEdgesTypeConverter();

			// Simulate what happens when binding encounters a string value
			// and needs to convert it to SafeAreaEdges

			// Test single value conversion (what binding would do)
			var allResult = converter.ConvertFrom(null, null, "All");
			Assert.Equal(SafeAreaEdges.All, allResult);

			// Test two values conversion
			var twoValueResult = converter.ConvertFrom(null, null, "SoftInput,Container");
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.SoftInput, SafeAreaRegions.Container), twoValueResult);

			// Test four values conversion
			var fourValueResult = converter.ConvertFrom(null, null, "None,SoftInput,Container,All");
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.SoftInput, SafeAreaRegions.Container, SafeAreaRegions.All), fourValueResult);

			// Test that conversion back to string works (for two-way binding)
			var backToString = converter.ConvertTo(null, null, SafeAreaEdges.All, typeof(string));
			Assert.Equal("All, All, All, All", backToString);
		}

		[Fact]
		public void SafeAreaEdges_PropertyAssignment_WorksDirectly()
		{
			// Test direct property assignment (what would happen after successful binding conversion)
			var layout = new Grid();

			// Test setting from converted values (simulating what binding would do after conversion)
			layout.SafeAreaEdges = SafeAreaEdges.All;
			Assert.Equal(SafeAreaEdges.All, layout.SafeAreaEdges);

			layout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput, SafeAreaRegions.Container);
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.SoftInput, SafeAreaRegions.Container), layout.SafeAreaEdges);

			layout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.SoftInput, SafeAreaRegions.Container, SafeAreaRegions.All);
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.SoftInput, SafeAreaRegions.Container, SafeAreaRegions.All), layout.SafeAreaEdges);
		}

		#region Stack Layout Automatic Safe Area Tests

		[Fact]
		public void HorizontalStackLayout_BehavesLikeRegularLayout()
		{
			var stackLayout = new HorizontalStackLayout();

			// Should behave like regular layout with default SafeAreaEdges.None (edge-to-edge)
			var safeAreaView2 = (ISafeAreaView2)stackLayout;

			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(0)); // Left
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(1)); // Top
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(2)); // Right
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(3)); // Bottom
		}

		[Fact]
		public void HorizontalStackLayout_RespectsDirectProperty_RTL()
		{
			var stackLayout = new HorizontalStackLayout();
			stackLayout.FlowDirection = FlowDirection.RightToLeft;
			stackLayout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All);

			// Should respect SafeAreaEdges direct property
			var safeAreaView2 = (ISafeAreaView2)stackLayout;

			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(0)); // Left (set to None)
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(1));  // Top (set to All)
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(2));  // Right (set to All)
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(3));  // Bottom (set to All)
		}

		[Fact]
		public void VerticalStackLayout_BehavesLikeRegularLayout()
		{
			var stackLayout = new VerticalStackLayout();

			// Should behave like regular layout with default SafeAreaEdges.None (edge-to-edge)
			var safeAreaView2 = (ISafeAreaView2)stackLayout;

			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(0)); // Left
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(1)); // Top
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(2)); // Right
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(3)); // Bottom
		}

		[Fact]
		public void StackLayout_HorizontalOrientation_BehavesLikeRegularLayout_LTR()
		{
			var stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal };

			// Should behave like regular layout with default SafeAreaEdges.None (edge-to-edge)
			var safeAreaView2 = (ISafeAreaView2)stackLayout;

			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(0)); // Left
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(1)); // Top
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(2)); // Right
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(3)); // Bottom
		}

		[Fact]
		public void StackLayout_HorizontalOrientation_RespectsDirectProperty_RTL()
		{
			var stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
			stackLayout.FlowDirection = FlowDirection.RightToLeft;
			stackLayout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All);

			// Should respect SafeAreaEdges direct property
			var safeAreaView2 = (ISafeAreaView2)stackLayout;

			Assert.Equal(SafeAreaRegions.None, safeAreaView2.GetSafeAreaRegionsForEdge(0)); // Left (set to None)
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(1));  // Top (set to All)
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(2));  // Right (set to All)
			Assert.Equal(SafeAreaRegions.All, safeAreaView2.GetSafeAreaRegionsForEdge(3));  // Bottom (set to All)
		}

		[Fact]
		public void StackLayout_VerticalOrientation_BehavesLikeRegularLayout()
		{
			var stackLayout = new StackLayout { Orientation = StackOrientation.Vertical };

			// Should behave like regular layout with default SafeAreaEdges.None (edge-to-edge)
			var safeAreaView2 = (ISafeAreaView2)stackLayout;

			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(0)); // Left
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(1)); // Top
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(2)); // Right
			Assert.Equal(SafeAreaRegions.Container, safeAreaView2.GetSafeAreaRegionsForEdge(3)); // Bottom
		}

		[Fact]
		public void StackLayouts_RespectUserSettings()
		{
			var verticalStackLayout = new VerticalStackLayout();
			var horizontalStackLayout = new HorizontalStackLayout();

			// Set explicit SafeAreaEdges property - this should be respected
			verticalStackLayout.SafeAreaEdges = SafeAreaEdges.All; // Obey all edges
			horizontalStackLayout.SafeAreaEdges = SafeAreaEdges.None; // Edge-to-edge all edges

			var verticalSafeAreaView2 = (ISafeAreaView2)verticalStackLayout;
			var horizontalSafeAreaView2 = (ISafeAreaView2)horizontalStackLayout;

			// VerticalStackLayout should respect user setting (All = obey safe area)
			Assert.Equal(SafeAreaRegions.All, verticalSafeAreaView2.GetSafeAreaRegionsForEdge(0)); // Left
			Assert.Equal(SafeAreaRegions.All, verticalSafeAreaView2.GetSafeAreaRegionsForEdge(1)); // Top
			Assert.Equal(SafeAreaRegions.All, verticalSafeAreaView2.GetSafeAreaRegionsForEdge(2)); // Right
			Assert.Equal(SafeAreaRegions.All, verticalSafeAreaView2.GetSafeAreaRegionsForEdge(3)); // Bottom

			// HorizontalStackLayout should respect user setting (None = edge-to-edge)
			Assert.Equal(SafeAreaRegions.None, horizontalSafeAreaView2.GetSafeAreaRegionsForEdge(0)); // Left
			Assert.Equal(SafeAreaRegions.None, horizontalSafeAreaView2.GetSafeAreaRegionsForEdge(1)); // Top
			Assert.Equal(SafeAreaRegions.None, horizontalSafeAreaView2.GetSafeAreaRegionsForEdge(2)); // Right
			Assert.Equal(SafeAreaRegions.None, horizontalSafeAreaView2.GetSafeAreaRegionsForEdge(3)); // Bottom
		}

		#endregion
	}
}