using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class SafeAreaEdgesCompiledBindingTests
	{
		[TestCase(true)]
		[TestCase(false)]
		public void SafeAreaEdgesConversionsWork(bool useCompiledXaml)
		{
			var page = new TestSafeAreaEdgesPage(useCompiledXaml);
			
			// Test uniform value
			Assert.AreEqual(SafeAreaRegions.All, page.stackUniform.SafeAreaEdges.Left);
			Assert.AreEqual(SafeAreaRegions.All, page.stackUniform.SafeAreaEdges.Top);
			Assert.AreEqual(SafeAreaRegions.All, page.stackUniform.SafeAreaEdges.Right);
			Assert.AreEqual(SafeAreaRegions.All, page.stackUniform.SafeAreaEdges.Bottom);
			
			// Test horizontal/vertical values
			Assert.AreEqual(SafeAreaRegions.All, page.stackHorizontalVertical.SafeAreaEdges.Left);
			Assert.AreEqual(SafeAreaRegions.None, page.stackHorizontalVertical.SafeAreaEdges.Top);
			Assert.AreEqual(SafeAreaRegions.All, page.stackHorizontalVertical.SafeAreaEdges.Right);
			Assert.AreEqual(SafeAreaRegions.None, page.stackHorizontalVertical.SafeAreaEdges.Bottom);
			
			// Test all four edge values
			Assert.AreEqual(SafeAreaRegions.All, page.stackAllEdges.SafeAreaEdges.Left);
			Assert.AreEqual(SafeAreaRegions.None, page.stackAllEdges.SafeAreaEdges.Top);
			Assert.AreEqual(SafeAreaRegions.Container, page.stackAllEdges.SafeAreaEdges.Right);
			Assert.AreEqual(SafeAreaRegions.SoftInput, page.stackAllEdges.SafeAreaEdges.Bottom);
			
			// Test default value
			Assert.AreEqual(SafeAreaRegions.Default, page.scrollDefault.SafeAreaEdges.Left);
			Assert.AreEqual(SafeAreaRegions.Default, page.scrollDefault.SafeAreaEdges.Top);
			Assert.AreEqual(SafeAreaRegions.Default, page.scrollDefault.SafeAreaEdges.Right);
			Assert.AreEqual(SafeAreaRegions.Default, page.scrollDefault.SafeAreaEdges.Bottom);
		}
	}
}