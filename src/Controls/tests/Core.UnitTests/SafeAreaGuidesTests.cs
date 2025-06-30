using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class SafeAreaGuidesTests : BaseTestFixture
	{
		[Fact]
		public void GetIgnoreSafeArea_DefaultValue_ReturnsNoneArray()
		{
			var layout = new Grid();
			
			var result = SafeAreaGuides.GetIgnoreSafeArea(layout);
			
			Assert.NotNull(result);
			Assert.Single(result);
			Assert.Equal(SafeAreaGroup.None, result[0]);
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
		public void Layout_ImplementsISafeAreaView3()
		{
			var layout = new Grid();
			
			Assert.IsAssignableFrom<ISafeAreaView3>(layout);
		}

		[Fact]
		public void Layout_IgnoreSafeAreaForEdge_UsesAttachedProperty()
		{
			var layout = new Grid();
			SafeAreaGuides.SetIgnoreSafeArea(layout, new[] { SafeAreaGroup.All, SafeAreaGroup.None, SafeAreaGroup.All, SafeAreaGroup.None });

			// Test via ISafeAreaView3 interface
			var safeAreaView3 = (ISafeAreaView3)layout;
			
			Assert.True(safeAreaView3.IgnoreSafeAreaForEdge(0));  // Left = All
			Assert.False(safeAreaView3.IgnoreSafeAreaForEdge(1)); // Top = None  
			Assert.True(safeAreaView3.IgnoreSafeAreaForEdge(2));  // Right = All
			Assert.False(safeAreaView3.IgnoreSafeAreaForEdge(3)); // Bottom = None
		}

		[Fact]
		public void Layout_IgnoreSafeAreaForEdge_FallsBackToLegacyProperty()
		{
			var layout = new Grid();
			layout.IgnoreSafeArea = true; // Legacy property

			// Should fall back to legacy property when no attached property is set
			var safeAreaView3 = (ISafeAreaView3)layout;
			
			Assert.True(safeAreaView3.IgnoreSafeAreaForEdge(0));
			Assert.True(safeAreaView3.IgnoreSafeAreaForEdge(1));
			Assert.True(safeAreaView3.IgnoreSafeAreaForEdge(2));
			Assert.True(safeAreaView3.IgnoreSafeAreaForEdge(3));
		}
	}
}