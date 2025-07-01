using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class SafeAreaGuidesTests : BaseTestFixture
	{
		[Fact]
		public void GetIgnoreSafeArea_DefaultValue_ReturnsAllArray()
		{
			var layout = new Grid();
			
			var result = SafeAreaGuides.GetIgnoreSafeArea(layout);
			
			Assert.NotNull(result);
			Assert.Single(result);
			Assert.Equal(SafeAreaGroup.All, result[0]);
		}

		[Fact]
		public void SetIgnoreSafeArea_SingleValue_AppliedCorrectly()
		{
			var layout = new Grid();
			var value = new[] { SafeAreaGroup.All };
			
			SafeAreaGuides.SetIgnoreSafeArea(layout, value);
			var result = SafeAreaGuides.GetIgnoreSafeArea(layout);
			
			Assert.Equal(value, result);
		}

		[Fact]
		public void GetIgnoreSafeAreaForEdge_SingleValue_AppliesAllEdges()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, new[] { SafeAreaGroup.All });
			
			Assert.Equal(SafeAreaGroup.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 0)); // Left
			Assert.Equal(SafeAreaGroup.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 1)); // Top
			Assert.Equal(SafeAreaGroup.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 2)); // Right
			Assert.Equal(SafeAreaGroup.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 3)); // Bottom
		}

		[Fact]
		public void GetIgnoreSafeAreaForEdge_TwoValues_AppliesCorrectly()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, new[] { SafeAreaGroup.All, SafeAreaGroup.None });
			
			Assert.Equal(SafeAreaGroup.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 0)); // Left
			Assert.Equal(SafeAreaGroup.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 1)); // Top
			Assert.Equal(SafeAreaGroup.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 2)); // Right
			Assert.Equal(SafeAreaGroup.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 3)); // Bottom
		}

		[Fact]
		public void GetIgnoreSafeAreaForEdge_FourValues_AppliesCorrectly()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, new[] { 
				SafeAreaGroup.All,   // Left
				SafeAreaGroup.None,  // Top
				SafeAreaGroup.All,   // Right
				SafeAreaGroup.None   // Bottom
			});
			
			Assert.Equal(SafeAreaGroup.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 0)); // Left
			Assert.Equal(SafeAreaGroup.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 1)); // Top
			Assert.Equal(SafeAreaGroup.All, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 2)); // Right
			Assert.Equal(SafeAreaGroup.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 3)); // Bottom
		}

		[Fact]
		public void GetIgnoreSafeAreaForEdge_UnsupportedArrayLength_ReturnsNone()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, new[] { SafeAreaGroup.All, SafeAreaGroup.None, SafeAreaGroup.All }); // 3 values not supported
			
			Assert.Equal(SafeAreaGroup.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 0));
			Assert.Equal(SafeAreaGroup.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 1));
			Assert.Equal(SafeAreaGroup.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 2));
			Assert.Equal(SafeAreaGroup.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 3));
		}

		[Fact]
		public void GetIgnoreSafeAreaForEdge_NullArray_ReturnsNone()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, null);
			
			Assert.Equal(SafeAreaGroup.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 0));
		}

		[Fact]
		public void GetIgnoreSafeAreaForEdge_EmptyArray_ReturnsNone()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, System.Array.Empty<SafeAreaGroup>());
			
			Assert.Equal(SafeAreaGroup.None, SafeAreaGuides.GetIgnoreSafeAreaForEdge(layout, 0));
		}

		[Fact]
		public void Layout_ImplementsISafeAreaView2()
		{
			var layout = new Grid();
			
			Assert.IsAssignableFrom<ISafeAreaView2>(layout);
		}

		[Fact]
		public void Layout_IgnoreSafeAreaForEdge_UsesAttachedProperty()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, new[] { SafeAreaGroup.All, SafeAreaGroup.None, SafeAreaGroup.All, SafeAreaGroup.None });

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
		public void Page_ImplementsISafeAreaView2()
		{
			var page = new ContentPage();
			
			Assert.IsAssignableFrom<ISafeAreaView2>(page);
		}

		[Fact]
		public void ContentView_ImplementsISafeAreaView2()
		{
			var contentView = new ContentView();
			
			Assert.IsAssignableFrom<ISafeAreaView2>(contentView);
		}

		[Fact]
		public void Page_IgnoreSafeAreaForEdge_UsesAttachedProperty()
		{
			var page = new ContentPage();
			SafeAreaGuides.SetIgnoreSafeArea(page, new[] { SafeAreaGroup.All, SafeAreaGroup.None, SafeAreaGroup.All, SafeAreaGroup.None });

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
			var contentView = new ContentView(); // ContentView doesn't implement ISafeAreaView

			// Should default to true when no attached property is set and no legacy support (default is now SafeAreaGroup.All)
			var safeAreaView2 = (ISafeAreaView2)contentView;
			
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
			SafeAreaGuides.SetIgnoreSafeArea(contentView, new[] { SafeAreaGroup.None }); // Respect all safe areas

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
			SafeAreaGuides.SetIgnoreSafeArea(stackLayout, new[] { SafeAreaGroup.None, SafeAreaGroup.All, SafeAreaGroup.None, SafeAreaGroup.All });

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
			
			SafeAreaGuides.SetIgnoreSafeArea(grid, new[] { SafeAreaGroup.All, SafeAreaGroup.None });

			var safeAreaView2 = (ISafeAreaView2)grid;
			
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(0));  // Left = All  
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(1)); // Top = None
			Assert.True(safeAreaView2.IgnoreSafeAreaForEdge(2));  // Right = All
			Assert.False(safeAreaView2.IgnoreSafeAreaForEdge(3)); // Bottom = None
		}
	}
}