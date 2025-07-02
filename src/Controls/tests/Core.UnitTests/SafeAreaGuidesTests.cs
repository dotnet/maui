using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class SafeAreaGuidesTests : BaseTestFixture
	{
		[Fact]
		public void GetIgnoreSafeArea_DefaultValue_ReturnsNone()
		{
			var layout = new Grid();
			
			var result = SafeAreaGuides.GetIgnoreSafeArea(layout);
			
			Assert.Equal(SafeAreaEdges.None, result);
		}

		[Fact]
		public void SetIgnoreSafeArea_SingleValue_AppliedCorrectly()
		{
			var layout = new Grid();
			var value = SafeAreaEdges.All;
			
			SafeAreaGuides.SetIgnoreSafeArea(layout, value);
			var result = SafeAreaGuides.GetIgnoreSafeArea(layout);
			
			Assert.Equal(value, result);
		}

		[Fact]
		public void GetIgnoreSafeAreaForEdge_UniformValue_AppliesAllEdges()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, SafeAreaEdges.All);
			
			Assert.Equal(SafeAreaRegions.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 0)); // Left
			Assert.Equal(SafeAreaRegions.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 1)); // Top
			Assert.Equal(SafeAreaRegions.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 2)); // Right
			Assert.Equal(SafeAreaRegions.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 3)); // Bottom
		}

		[Fact]
		public void GetIgnoreSafeAreaForEdge_TwoValues_AppliesCorrectly()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None));
			
			Assert.Equal(SafeAreaRegions.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 0)); // Left
			Assert.Equal(SafeAreaRegions.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 1)); // Top
			Assert.Equal(SafeAreaRegions.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 2)); // Right
			Assert.Equal(SafeAreaRegions.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 3)); // Bottom
		}

		[Fact]
		public void GetIgnoreSafeAreaForEdge_FourValues_AppliesCorrectly()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, new SafeAreaEdges(
				SafeAreaRegions.All,   // Left
				SafeAreaRegions.None,  // Top
				SafeAreaRegions.All,   // Right
				SafeAreaRegions.None   // Bottom
			));
			
			Assert.Equal(SafeAreaRegions.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 0)); // Left
			Assert.Equal(SafeAreaRegions.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 1)); // Top
			Assert.Equal(SafeAreaRegions.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 2)); // Right
			Assert.Equal(SafeAreaRegions.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 3)); // Bottom
		}

		[Fact]
		public void GetIgnoreSafeAreaForEdge_InvalidEdgeIndex_ReturnsNone()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, SafeAreaEdges.All);
			
			Assert.Equal(SafeAreaRegions.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, -1));
			Assert.Equal(SafeAreaRegions.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 4));
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
			
			Assert.Equal("All", uniform.ToString());
			Assert.Equal("All, None", twoValue.ToString());
			Assert.Equal("All, None, All, None", fourValue.ToString());
		}

		[Fact]
		public void Layout_ImplementsISafeAreaView()
		{
			var layout = new Grid();
			
			Assert.IsAssignableFrom<ISafeAreaView>(layout);
			Assert.IsAssignableFrom<ISafeAreaView2>(layout);
		}

		[Fact]
		public void Layout_IgnoreSafeAreaForEdge_UsesAttachedProperty()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None));

			// Test via ISafeAreaView2 interface
			var safeAreaView2 = (ISafeAreaView2)layout;
			
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
			var safeAreaView2 = (ISafeAreaView2)layout;
			
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
			Assert.IsAssignableFrom<ISafeAreaView2>(page);
		}

		[Fact]
		public void ContentView_ImplementsISafeAreaView()
		{
			var contentView = new ContentView();
			
			Assert.IsAssignableFrom<ISafeAreaView2>(contentView);
		}

		[Fact]
		public void Page_IgnoreSafeAreaForEdge_UsesAttachedProperty()
		{
			var page = new ContentPage();
			SafeAreaGuides.SetIgnoreSafeArea(page, new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None));

			// Test via ISafeAreaView2 interface
			var safeAreaView2 = (ISafeAreaView2)page;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));  // Left = All
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top = None  
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2));  // Right = All
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom = None
		}

		[Fact]
		public void ContentView_IgnoreSafeAreaForEdge_FallsBackToDefaultWhenNoLegacySupport()
		{
			var contentView = new ContentView(); // ContentView implements ISafeAreaView2

			// Should default to false when no attached property is set and no legacy support (default is now SafeAreaRegions.None)
			var safeAreaView2 = (ISafeAreaView2)contentView;
			
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(0));
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1));
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2));
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3));
		}

		[Fact]
		public void Page_IgnoreSafeAreaForEdge_DefaultsToAllWhenNoPropertySet()
		{
			var page = new ContentPage(); // Page defaults to All when no property is explicitly set

			// Should default to true for Page even though SafeAreaGuides default is None
			var safeAreaView2 = (ISafeAreaView2)page;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(1));
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2));
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(3));
		}

		// Tests based on existing iOS safe area usage patterns
		[Fact]
		public void SafeAreaGuides_CanReplaceUseSafeAreaScenario()
		{
			// This test mimics the usage pattern from Issue3809 and ShellTests.iOS
			var contentView = new ContentView() 
			{ 
				Content = new Label() { Text = "Test Page" }
			};

			// Legacy approach: contentPage.On<iOS>().SetUseSafeArea(true);
			// New approach: use SafeAreaGuides attached property
			SafeAreaGuides.SetIgnoreSafeArea(contentView, SafeAreaEdges.None); // Respect all safe areas

			var safeAreaView2 = (ISafeAreaView2)contentView;
			
			// All edges should respect safe area (false)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(0));
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1));
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2));
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3));
		}

		[Fact]
		public void SafeAreaGuides_SupportsPerEdgeControl_IgnoreTopAndBottom()
		{
			// Common use case: ignore safe area for top and bottom edges only (e.g., for full-width background)
			var stackLayout = new VerticalStackLayout();
			
			// Ignore safe area for top and bottom, respect for left and right
			SafeAreaGuides.SetIgnoreSafeArea(stackLayout, new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All));

			var safeAreaView2 = (ISafeAreaView2)stackLayout;
			
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(0)); // Left = None (respect)
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(1));  // Top = All (ignore)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2)); // Right = None (respect)  
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(3));  // Bottom = All (ignore)
		}

		[Fact]
		public void SafeAreaGuides_TwoValueShorthand_LeftRightTopBottom()
		{
			// Test two-value shorthand: first value for left/right, second for top/bottom
			var grid = new Grid();
			
			SafeAreaGuides.SetIgnoreSafeArea(grid, new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None));

			var safeAreaView2 = (ISafeAreaView2)grid;
			
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
	}
}