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
		public void GetIgnore_DefaultValue_ReturnsDefault()
		{
			var layout = new Grid();
			
			var result = SafeAreaElement.GetIgnore(layout);
			
			Assert.Equal(SafeAreaEdges.Default, result);
		}

		[Fact]
		public void SetIgnore_SingleValue_AppliedCorrectly()
		{
			var layout = new Grid();
			var value = SafeAreaEdges.All;
			
			SafeAreaElement.SetIgnore(layout, value);
			var result = SafeAreaElement.GetIgnore(layout);
			
			Assert.Equal(value, result);
		}

		[Fact]
		public void GetIgnoreForEdge_UniformValue_AppliesAllEdges()
		{
			var layout = new Grid();
			SafeAreaElement.SetIgnore(layout, SafeAreaEdges.All);
			
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetIgnoreForEdge(layout, 0)); // Left
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetIgnoreForEdge(layout, 1)); // Top
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetIgnoreForEdge(layout, 2)); // Right
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetIgnoreForEdge(layout, 3)); // Bottom
		}

		[Fact]
		public void GetIgnoreForEdge_TwoValues_AppliesCorrectly()
		{
			var layout = new Grid();
			SafeAreaElement.SetIgnore(layout, new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None));
			
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetIgnoreForEdge(layout, 0)); // Left
			Assert.Equal(SafeAreaRegions.None, SafeAreaElement.GetIgnoreForEdge(layout, 1)); // Top
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetIgnoreForEdge(layout, 2)); // Right
			Assert.Equal(SafeAreaRegions.None, SafeAreaElement.GetIgnoreForEdge(layout, 3)); // Bottom
		}

		[Fact]
		public void GetIgnoreForEdge_FourValues_AppliesCorrectly()
		{
			var layout = new Grid();
			SafeAreaElement.SetIgnore(layout, new SafeAreaEdges(
				SafeAreaRegions.All,   // Left
				SafeAreaRegions.None,  // Top
				SafeAreaRegions.All,   // Right
				SafeAreaRegions.None   // Bottom
			));
			
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetIgnoreForEdge(layout, 0)); // Left
			Assert.Equal(SafeAreaRegions.None, SafeAreaElement.GetIgnoreForEdge(layout, 1)); // Top
			Assert.Equal(SafeAreaRegions.All, SafeAreaElement.GetIgnoreForEdge(layout, 2)); // Right
			Assert.Equal(SafeAreaRegions.None, SafeAreaElement.GetIgnoreForEdge(layout, 3)); // Bottom
		}

		[Fact]
		public void GetIgnoreForEdge_InvalidEdgeIndex_ReturnsNone()
		{
			var layout = new Grid();
			SafeAreaElement.SetIgnore(layout, SafeAreaEdges.All);
			
			Assert.Equal(SafeAreaRegions.None, SafeAreaElement.GetIgnoreForEdge(layout, -1));
			Assert.Equal(SafeAreaRegions.None, SafeAreaElement.GetIgnoreForEdge(layout, 4));
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
		public void SafeAreaEdges_SetEdge_UpdatesCorrectValues()
		{
			var edges = new SafeAreaEdges();
			
			edges.SetEdge(0, SafeAreaRegions.All); // Left
			edges.SetEdge(1, SafeAreaRegions.None); // Top
			edges.SetEdge(2, SafeAreaRegions.All); // Right
			edges.SetEdge(3, SafeAreaRegions.None); // Bottom
			
			Assert.Equal(SafeAreaRegions.All, edges.Left);
			Assert.Equal(SafeAreaRegions.None, edges.Top);
			Assert.Equal(SafeAreaRegions.All, edges.Right);
			Assert.Equal(SafeAreaRegions.None, edges.Bottom);
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
			
			Assert.Equal("All", uniform.ToString());
			Assert.Equal("All, None", twoValue.ToString());
			Assert.Equal("All, None", fourValue.ToString()); // Optimized: Left==Right, Top==Bottom
			Assert.Equal("All, None, None, All", asymmetric.ToString()); // No optimization possible
		}

		[Fact]
		public void Layout_ImplementsISafeAreaView()
		{
			var layout = new Grid();
			
			Assert.IsAssignableFrom<ISafeAreaView>(layout);
			Assert.IsAssignableFrom<ISafeAreaPage>(layout);
		}

		[Fact]
		public void Layout_IgnoreSafeAreaForEdge_UsesAttachedProperty()
		{
			var layout = new Grid();
			SafeAreaElement.SetIgnore(layout, new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None));

			// Test via ISafeAreaPage interface
			var safeAreaView2 = (ISafeAreaPage)layout;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));  // Left = All
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top = None  
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2));  // Right = All
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom = None
		}

		[Fact]
		public void Layout_IgnoreSafeAreaForEdge_FallsBackToLegacyProperty()
		{
			var layout = new Grid();
			layout.IgnoreSafeArea = true; // Legacy property

			// Should fall back to legacy property when no attached property is set
			var safeAreaView2 = (ISafeAreaPage)layout;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(1));
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2));
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(3));
		}

		[Fact]
		public void Page_ImplementsISafeAreaView()
		{
			var page = new ContentPage();
			
			Assert.IsAssignableFrom<ISafeAreaView>(page);
			Assert.IsAssignableFrom<ISafeAreaPage>(page);
		}

		[Fact]
		public void ContentView_ImplementsISafeAreaView()
		{
			var contentView = new ContentView();
			
			Assert.IsAssignableFrom<ISafeAreaPage>(contentView);
		}

		[Fact]
		public void Page_IgnoreSafeAreaForEdge_UsesAttachedProperty()
		{
			var page = new ContentPage();
			SafeAreaElement.SetIgnore(page, new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None));

			// Test via ISafeAreaPage interface
			var safeAreaView2 = (ISafeAreaPage)page;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));  // Left = All
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top = None  
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2));  // Right = All
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom = None
		}

		[Fact]
		public void ContentView_IgnoreSafeAreaForEdge_FallsBackToDefaultWhenNoLegacySupport()
		{
			var contentView = new ContentView(); // ContentView implements ISafeAreaPage

			// Should default to false when no attached property is set and no legacy support (default is now SafeAreaRegions.None)
			var safeAreaView2 = (ISafeAreaPage)contentView;
			
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(0));
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1));
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2));
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3));
		}

		[Fact]
		public void Page_IgnoreSafeAreaForEdge_DefaultsToAllWhenNoPropertySet()
		{
			var page = new ContentPage(); // Page defaults to All when no property is explicitly set

			// Should default to true for Page even though SafeArea default is None
			var safeAreaView2 = (ISafeAreaPage)page;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(1));
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2));
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(3));
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
			// New approach: use SafeArea attached property
			SafeAreaElement.SetIgnore(contentView, SafeAreaEdges.None); // Respect all safe areas

			var safeAreaView2 = (ISafeAreaPage)contentView;
			
			// All edges should respect safe area (false)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(0));
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1));
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2));
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3));
		}

		[Fact]
		public void SafeArea_SupportsPerEdgeControl_IgnoreTopAndBottom()
		{
			// Common use case: ignore safe area for top and bottom edges only (e.g., for full-width background)
			var stackLayout = new VerticalStackLayout();
			
			// Ignore safe area for top and bottom, respect for left and right
			SafeAreaElement.SetIgnore(stackLayout, new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All));

			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(0)); // Left = None (respect)
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(1));  // Top = All (ignore)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2)); // Right = None (respect)  
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(3));  // Bottom = All (ignore)
		}

		[Fact]
		public void SafeArea_TwoValueShorthand_LeftRightTopBottom()
		{
			// Test two-value shorthand: first value for left/right, second for top/bottom
			var grid = new Grid();
			
			SafeAreaElement.SetIgnore(grid, new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None));

			var safeAreaView2 = (ISafeAreaPage)grid;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));  // Left = All  
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top = None
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2));  // Right = All
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom = None
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
			
			var result = (SafeAreaEdges)converter.ConvertFrom(null, null, "All,None");
			
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None), result);
		}

		[Fact]
		public void SafeAreaEdgesTypeConverter_ConvertFromFourValues()
		{
			var converter = new SafeAreaEdgesTypeConverter();
			
			var result = (SafeAreaEdges)converter.ConvertFrom(null, null, "None,All,None,All");
			
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All), result);
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
				converter.ConvertFrom(null, null, "All,None,All")); // 3 values not supported
		}

		[Fact]
		public void SafeAreaEdgesTypeConverter_ConvertToString()
		{
			var converter = new SafeAreaEdgesTypeConverter();
			var edges = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None);
			
			var result = converter.ConvertTo(null, null, edges, typeof(string));
			
			Assert.Equal("All, None", result);
		}

		#region Stack Layout Automatic Safe Area Tests

		[Fact]
		public void HorizontalStackLayout_BehavesLikeRegularLayout()
		{
			var stackLayout = new HorizontalStackLayout();
			
			// Should behave like regular layout with default SafeAreaEdges.None (respect safe area)
			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(0)); // Left
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top  
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2)); // Right
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom
		}

		[Fact]
		public void HorizontalStackLayout_RespectsAttachedProperty_RTL()
		{
			var stackLayout = new HorizontalStackLayout();
			stackLayout.FlowDirection = FlowDirection.RightToLeft;
			SafeAreaElement.SetIgnore(stackLayout, new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None));
			
			// Should respect SafeAreaElement.Ignore attached property
			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));   // Left (set to All)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1));  // Top (set to None)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2));  // Right (set to None)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3));  // Bottom (set to None)
		}

		[Fact]
		public void VerticalStackLayout_BehavesLikeRegularLayout()
		{
			var stackLayout = new VerticalStackLayout();
			
			// Should behave like regular layout with default SafeAreaEdges.None (respect safe area)
			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(0)); // Left
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top  
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2)); // Right
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom
		}

		[Fact]
		public void StackLayout_HorizontalOrientation_BehavesLikeRegularLayout_LTR()
		{
			var stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
			
			// Should behave like regular layout with default SafeAreaEdges.None (respect safe area)
			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(0)); // Left
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top  
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2)); // Right
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom
		}

		[Fact]
		public void StackLayout_HorizontalOrientation_RespectsAttachedProperty_RTL()
		{
			var stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
			stackLayout.FlowDirection = FlowDirection.RightToLeft;
			SafeAreaElement.SetIgnore(stackLayout, new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None));
			
			// Should respect SafeAreaElement.Ignore attached property
			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));   // Left (set to All)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1));  // Top (set to None)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2));  // Right (set to None)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3));  // Bottom (set to None)
		}

		[Fact]
		public void StackLayout_VerticalOrientation_BehavesLikeRegularLayout()
		{
			var stackLayout = new StackLayout { Orientation = StackOrientation.Vertical };
			
			// Should behave like regular layout with default SafeAreaEdges.None (respect safe area)
			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(0)); // Left
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top  
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2)); // Right
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom
		}

		[Fact]
		public void StackLayouts_RespectUserSettings()
		{
			var verticalStackLayout = new VerticalStackLayout();
			var horizontalStackLayout = new HorizontalStackLayout();
			
			// Set explicit SafeAreaElement.Ignore property - this should be respected
			SafeAreaElement.SetIgnore(verticalStackLayout, SafeAreaEdges.None); // Respect all edges
			SafeAreaElement.SetIgnore(horizontalStackLayout, SafeAreaEdges.All); // Ignore all edges
			
			var verticalSafeAreaView2 = (ISafeAreaPage)verticalStackLayout;
			var horizontalSafeAreaView2 = (ISafeAreaPage)horizontalStackLayout;
			
			// VerticalStackLayout should respect user setting (None = respect safe area)
			Assert.False(verticalSafeAreaView2.IgnoreSafeAreaForEdge(0)); // Left (respects safe area)
			Assert.False(verticalSafeAreaView2.IgnoreSafeAreaForEdge(1)); // Top (respects safe area)
			Assert.False(verticalSafeAreaView2.IgnoreSafeAreaForEdge(2)); // Right (respects safe area)
			Assert.False(verticalSafeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom (respects safe area)
			
			// HorizontalStackLayout should respect user setting (All = ignore safe area)
			Assert.True(horizontalSafeAreaView2.IgnoreSafeAreaForEdge(0)); // Left (ignores safe area)
			Assert.True(horizontalSafeAreaView2.IgnoreSafeAreaForEdge(1)); // Top (ignores safe area)
			Assert.True(horizontalSafeAreaView2.IgnoreSafeAreaForEdge(2)); // Right (ignores safe area)
			Assert.True(horizontalSafeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom (ignores safe area)
		}

		#endregion
	}
}