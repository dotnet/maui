using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.StyleSheets.UnitTests
{

	public class IStylableTest
	{
		[Fact]
		public void GetPropertyDefinedOnParent()
		{
			var label = new Label();
			var bp = ((IStylable)label).GetProperty("background-color", false);
			Assert.Same(VisualElement.BackgroundColorProperty, bp);
		}

		[Fact]
		public void GetPropertyDefinedOnType()
		{
			var label = new Label();
			var bp = ((IStylable)label).GetProperty("color", false);
			Assert.Same(Label.TextColorProperty, bp);
		}

		[Fact]
		public void GetPropertyDefinedOnType2()
		{
			var entry = new Entry();
			var bp = ((IStylable)entry).GetProperty("color", false);
			Assert.Same(Entry.TextColorProperty, bp);
		}

		[Fact]
		public void GetPropertyDefinedOnType3()
		{
			var indicator = new ActivityIndicator();
			var bp = ((IStylable)indicator).GetProperty("color", false);
			Assert.Same(ActivityIndicator.ColorProperty, bp);
		}

		[Fact]
		public void GetInvalidPropertyForType()
		{
			var grid = new Grid();
			var bp = ((IStylable)grid).GetProperty("color", false);
			Assert.Null(bp);
		}

		[Fact]
		public void GetPropertyDefinedOnPropertyOwnerType()
		{
			var frame = new Frame();
			var bp = ((IStylable)frame).GetProperty("padding-left", false);
			Assert.Same(bp, PaddingElement.PaddingLeftProperty);
		}

		[Fact]
		public void GetNonPublicProperty()
		{
			var label = new Label();
			var bp = ((IStylable)label).GetProperty("margin-right", false);
			Assert.Same(bp, View.MarginRightProperty);
		}
	}
}