using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Tests
{
	public class Color_Tests
	{
		[Fact]
		public void ToUInt()
		{
			var color = Color.FromRgba(255, 122, 15, 255);
			var i = color.ToUInt();
			Assert.Equal(4294933007U, i);
		}

		[Theory]
		[InlineData("#FF0000", "#00FFFF")] // Red & Cyan
		[InlineData("#00FF00", "#FF00FF")] // Green & Fuchsia
		[InlineData("#0000FF", "#FFFF00")] // Blue & Yellow
		[InlineData("#0AF56C", "#F50A93")] // Lime green & bright purple (but with no limit values)
		public void GetComplementary(string original, string expected)
		{
			var orig = Color.FromHex(original);
			var expectedComplement = Color.FromHex(expected);

			Assert.Equal(expectedComplement, orig.GetComplementary());
		}
	}
}
