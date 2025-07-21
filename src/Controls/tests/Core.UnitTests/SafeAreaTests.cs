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
			
			Assert.Equal(SafeAreaEdges.Default, result);
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
		public void Layout_IgnoreSafeAreaForEdge_UsesDirectProperty()
		{
			var layout = new Grid();
			layout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All);

			// Test via ISafeAreaPage interface - inverted logic: None means edge-to-edge (ignore), All means obey
			var safeAreaView2 = (ISafeAreaPage)layout;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));  // Left = None (edge-to-edge, so ignore)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top = All (obey safe area, so don't ignore)  
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2));  // Right = None (edge-to-edge, so ignore)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom = All (obey safe area, so don't ignore)
		}

		[Fact]
		public void Layout_IgnoreSafeAreaForEdge_FallsBackToLegacyProperty()
		{
			var layout = new Grid();
			layout.IgnoreSafeArea = true; // Legacy property

			// Should fall back to legacy property when no direct property is set
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
		public void Page_IgnoreSafeAreaForEdge_UsesDirectProperty()
		{
			var page = new ContentPage();
			page.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All);

			// Test via ISafeAreaPage interface - inverted logic: None means edge-to-edge (ignore), All means obey
			var safeAreaView2 = (ISafeAreaPage)page;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));  // Left = None (edge-to-edge, so ignore)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top = All (obey safe area, so don't ignore)  
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2));  // Right = None (edge-to-edge, so ignore)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom = All (obey safe area, so don't ignore)
		}

		[Fact]
		public void ContentView_IgnoreSafeAreaForEdge_DefaultsToNoneWhenNoPropertySet()
		{
			var contentView = new ContentView(); // ContentView implements ISafeAreaPage

			// Should default to true (ignore) when no property is set since default is SafeAreaRegions.None (edge-to-edge)
			var safeAreaView2 = (ISafeAreaPage)contentView;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(1));
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2));
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(3));
		}

		[Fact]
		public void Page_IgnoreSafeAreaForEdge_DefaultsToNoneForContentPage()
		{
			var page = new ContentPage(); // ContentPage returns All when SafeAreaRegions.None is specified for better UX

			// ContentPage has special logic - when SafeAreaRegions.None is specified, it returns All for better UX
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
			// New approach: use SafeAreaEdges property to obey all safe areas
			contentView.SafeAreaEdges = SafeAreaEdges.All; // Obey all safe areas

			var safeAreaView2 = (ISafeAreaPage)contentView;
			
			// All edges should obey safe area (false = don't ignore)
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
			
			// Edge-to-edge for top and bottom (None), obey for left and right (All)
			stackLayout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None);

			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(0)); // Left = All (obey safe area, so don't ignore)
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(1));  // Top = None (edge-to-edge, so ignore)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2)); // Right = All (obey safe area, so don't ignore)
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(3));  // Bottom = None (edge-to-edge, so ignore)
		}

		[Fact]
		public void SafeArea_TwoValueShorthand_LeftRightTopBottom()
		{
			// Test two-value shorthand: first value for left/right, second for top/bottom
			var grid = new Grid();
			
			grid.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All);

			var safeAreaView2 = (ISafeAreaPage)grid;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));  // Left = None (edge-to-edge, so ignore)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top = All (obey safe area, so don't ignore)
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2));  // Right = None (edge-to-edge, so ignore)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom = All (obey safe area, so don't ignore)
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
			
			var result = (SafeAreaEdges)converter.ConvertFrom(null, null, "None,All,Container,Keyboard");
			
			Assert.Equal(new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.Container, SafeAreaRegions.Keyboard), result);
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
			
			Assert.Equal("None, Container", result);
		}

		#region Stack Layout Automatic Safe Area Tests

		[Fact]
		public void HorizontalStackLayout_BehavesLikeRegularLayout()
		{
			var stackLayout = new HorizontalStackLayout();
			
			// Should behave like regular layout with default SafeAreaEdges.None (edge-to-edge)
			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0)); // Left
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top  
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2)); // Right
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom
		}

		[Fact]
		public void HorizontalStackLayout_RespectsDirectProperty_RTL()
		{
			var stackLayout = new HorizontalStackLayout();
			stackLayout.FlowDirection = FlowDirection.RightToLeft;
			stackLayout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All);
			
			// Should respect SafeAreaEdges direct property
			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));   // Left (set to None - edge-to-edge)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1));  // Top (set to All - obey)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2));  // Right (set to All - obey)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3));  // Bottom (set to All - obey)
		}

		[Fact]
		public void VerticalStackLayout_BehavesLikeRegularLayout()
		{
			var stackLayout = new VerticalStackLayout();
			
			// Should behave like regular layout with default SafeAreaEdges.None (edge-to-edge)
			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0)); // Left
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top  
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2)); // Right
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom
		}

		[Fact]
		public void StackLayout_HorizontalOrientation_BehavesLikeRegularLayout_LTR()
		{
			var stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
			
			// Should behave like regular layout with default SafeAreaEdges.None (edge-to-edge)
			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0)); // Left
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top  
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2)); // Right
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom
		}

		[Fact]
		public void StackLayout_HorizontalOrientation_RespectsDirectProperty_RTL()
		{
			var stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
			stackLayout.FlowDirection = FlowDirection.RightToLeft;
			stackLayout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All);
			
			// Should respect SafeAreaEdges direct property
			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));   // Left (set to None - edge-to-edge)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1));  // Top (set to All - obey)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(2));  // Right (set to All - obey)
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3));  // Bottom (set to All - obey)
		}

		[Fact]
		public void StackLayout_VerticalOrientation_BehavesLikeRegularLayout()
		{
			var stackLayout = new StackLayout { Orientation = StackOrientation.Vertical };
			
			// Should behave like regular layout with default SafeAreaEdges.None (edge-to-edge)
			var safeAreaView2 = (ISafeAreaPage)stackLayout;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0)); // Left
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top  
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2)); // Right
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom
		}

		[Fact]
		public void StackLayouts_RespectUserSettings()
		{
			var verticalStackLayout = new VerticalStackLayout();
			var horizontalStackLayout = new HorizontalStackLayout();
			
			// Set explicit SafeAreaEdges property - this should be respected
			verticalStackLayout.SafeAreaEdges = SafeAreaEdges.All; // Obey all edges
			horizontalStackLayout.SafeAreaEdges = SafeAreaEdges.None; // Edge-to-edge all edges
			
			var verticalSafeAreaView2 = (ISafeAreaPage)verticalStackLayout;
			var horizontalSafeAreaView2 = (ISafeAreaPage)horizontalStackLayout;
			
			// VerticalStackLayout should respect user setting (All = obey safe area)
			Assert.False(verticalSafeAreaView2.IgnoreSafeAreaForEdge(0)); // Left (obeys safe area)
			Assert.False(verticalSafeAreaView2.IgnoreSafeAreaForEdge(1)); // Top (obeys safe area)
			Assert.False(verticalSafeAreaView2.IgnoreSafeAreaForEdge(2)); // Right (obeys safe area)
			Assert.False(verticalSafeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom (obeys safe area)
			
			// HorizontalStackLayout should respect user setting (None = edge-to-edge)
			Assert.True(horizontalSafeAreaView2.IgnoreSafeAreaForEdge(0)); // Left (edge-to-edge)
			Assert.True(horizontalSafeAreaView2.IgnoreSafeAreaForEdge(1)); // Top (edge-to-edge)
			Assert.True(horizontalSafeAreaView2.IgnoreSafeAreaForEdge(2)); // Right (edge-to-edge)
			Assert.True(horizontalSafeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom (edge-to-edge)
		}

		#endregion
	}
}