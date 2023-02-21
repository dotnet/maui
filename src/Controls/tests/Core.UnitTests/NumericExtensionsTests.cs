using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class NumericExtensionsTests
	{
		[Fact]
		public void InRange()
		{
			Assert.Equal(5, 5.Clamp(0, 10));
		}

		[Fact]
		public void BelowMin()
		{
			Assert.Equal(5, 0.Clamp(5, 10));
		}

		[Fact]
		public void AboveMax()
		{
			Assert.Equal(5, 10.Clamp(0, 5));
		}

		[Fact]
		public void MinMaxWrong()
		{
			Assert.Equal(0, 10.Clamp(5, 0));
			Assert.Equal(0, 5.Clamp(10, 0));
			Assert.Equal(5, 0.Clamp(10, 5));
		}
	}
}
