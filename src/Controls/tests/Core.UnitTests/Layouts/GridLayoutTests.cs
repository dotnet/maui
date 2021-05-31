using System.Collections.Generic;
using Microsoft.Maui.Controls.Layout2;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	public class GridLayoutTests
	{
		[Test]
		public void RemovedMauiViewsHaveNoRowColumnInfo()
		{
			var gl = new GridLayout();
			var view = NSubstitute.Substitute.For<IView>();

			gl.Add(view);
			gl.SetRow(view, 2);

			// Check our assumptions
			Assert.AreEqual(2, gl.GetRow(view));

			// Okay, removing the View from the Grid should mean that any attempt to get row/column info
			// for that View should fail
			gl.Remove(view);

			Assert.Throws(typeof(KeyNotFoundException), () => gl.GetRow(view));
			Assert.Throws(typeof(KeyNotFoundException), () => gl.GetRowSpan(view));
			Assert.Throws(typeof(KeyNotFoundException), () => gl.GetColumn(view));
			Assert.Throws(typeof(KeyNotFoundException), () => gl.GetColumnSpan(view));
		}

		[Test]
		public void AddedViewGetsDefaultRowAndColumn()
		{
			var gl = new GridLayout();
			var view = new Label();

			gl.Add(view);
			Assert.AreEqual(0, gl.GetRow(view));
			Assert.AreEqual(0, gl.GetColumn(view));
			Assert.AreEqual(1, gl.GetRowSpan(view));
			Assert.AreEqual(1, gl.GetColumnSpan(view));
		}

		[Test]
		public void AddedMauiViewGetsDefaultRowAndColumn()
		{
			var gl = new GridLayout();
			var view = NSubstitute.Substitute.For<IView>();

			gl.Add(view);
			Assert.AreEqual(0, gl.GetRow(view));
			Assert.AreEqual(0, gl.GetColumn(view));
			Assert.AreEqual(1, gl.GetRowSpan(view));
			Assert.AreEqual(1, gl.GetColumnSpan(view));
		}
	}
}
