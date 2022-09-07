using Microsoft.Maui.Graphics.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Graphics.Tests
{
	public class RectTypeConverterTests
	{

		[Theory]
		[MemberData(nameof(RectangleConvertData))]
		public void ConvertsRectangleFromString(string from, bool expectedSuccess, Rect expectedResult)
		{
			var ok = from.TryConvertFrom<RectTypeConverter, Rect>(out var p);
			Assert.Equal(expectedSuccess, ok);

			if (expectedSuccess)
				Assert.Equal(expectedResult, p);
		}

		[Theory]
		[MemberData(nameof(RectangleFConvertData))]
		public void ConvertsRectangleFFromString(string from, bool expectedSuccess, RectF expectedResult)
		{
			var ok = from.TryConvertFrom<RectFTypeConverter, RectF>(out var p);
			Assert.Equal(expectedSuccess, ok);

			if (expectedSuccess)
				Assert.Equal(expectedResult, p);
		}

		// 0, 0
		public static IEnumerable<object[]> RectangleConvertData()
			=> new List<object[]>
			{
				new object[] { "1,2,3,4", true, new Rect(1, 2, 3, 4) },
				new object[] { "1.1,2.0,3,4.4", true, new Rect(1.1, 2, 3, 4.4) },

				new object[] { "0,0, 0, 0", true, new Rect(0, 0, 0, 0) },
				new object[] { "0, 0, 0, 0", true, new Rect(0, 0, 0, 0) },
				new object[] { "0,  0, 0, 0", true, new Rect(0, 0, 0, 0) },

				new object[] { "1,2, 0, 0", true, new Rect(1, 2, 0, 0) },
				new object[] { "1, 2, 0, 0", true, new Rect(1, 2, 0, 0) },
				new object[] { "1,  2, 0, 0", true, new Rect(1, 2, 0, 0) },

				new object[] { "0.0,0.0, 0, 0", true, new Rect(0, 0, 0, 0) },
				new object[] { "0.0, 0.0, 0, 0", true, new Rect(0, 0, 0, 0) },
				new object[] { "0.0,  0.0, 0, 0", true, new Rect(0, 0, 0, 0) },

				new object[] { "1.1,2.1, 0, 0", true, new Rect(1.1, 2.1, 0, 0) },
				new object[] { "1.1, 2.1, 0, 0", true, new Rect(1.1, 2.1, 0, 0) },
				new object[] { "1.1,  2.1, 0, 0", true, new Rect(1.1, 2.1, 0, 0) },

				new object[] { "0,-0, 0, 0", true, new Rect(0, 0, 0, 0) },
				new object[] { "-0, 0, 0, 0", true, new Rect(0, 0, 0, 0) },
				new object[] { "-0,  -0, 0, 0", true, new Rect(0, 0, 0, 0) },

				new object[] { "-1,2, 0, 0", true, new Rect(-1, 2, 0, 0) },
				new object[] { "1, -2, 0, 0", true, new Rect(1, -2, 0, 0) },
				new object[] { "-1,  -2, 0, 0", true, new Rect(-1, -2, 0, 0) },

				new object[] { "-0.0,0.0, 0, 0", true, new Rect(0, 0, 0, 0) },
				new object[] { "0.0, -0.0, 0, 0", true, new Rect(0, 0, 0, 0) },
				new object[] { "-0.0,  -0.0, 0, 0", true, new Rect(0, 0, 0, 0) },

				new object[] { "-1.1,2.1, 0, 0", true, new Rect(-1.1, 2.1, 0, 0) },
				new object[] { "1.1, -2.1, 0, 0", true, new Rect(1.1, -2.1, 0, 0) },
				new object[] { "-1.1,  -2.1, 0, 0", true, new Rect(-1.1, -2.1, 0, 0) },

				new object[] { ".1,0, 0, 0", true, new Rect(0.1, 0, 0, 0) },
				new object[] { "-.1, 1, 0, 0", true, new Rect(-0.1, 1, 0, 0) },
				new object[] { "1, 	 1, 0, 0", true, new Rect(1, 1, 0, 0) },

				new object[] { "0", false, new Rect(0, 0, 0, 0) },
				new object[] { ",1, 0, 0", false, new Rect(0, 1, 0, 0) },
				new object[] { "1,, 0, 0", false, new Rect(1, 0, 0, 0) },
				new object[] { "", false, new Rect(0, 0, 0, 0) },
			};


		public static IEnumerable<object[]> RectangleFConvertData()
			=> new List<object[]>
			{
				new object[] { "1,2,3,4", true, new RectF(1, 2, 3, 4) },
				new object[] { "1.1,2.0,3,4.4", true, new RectF(1.1f, 2, 3, 4.4f) },

				new object[] { "0,0, 0, 0", true, new RectF(0, 0, 0, 0) },
				new object[] { "0, 0, 0, 0", true, new RectF(0, 0, 0, 0) },
				new object[] { "0,  0, 0, 0", true, new RectF(0, 0, 0, 0) },

				new object[] { "1,2, 0, 0", true, new RectF(1, 2, 0, 0) },
				new object[] { "1, 2, 0, 0", true, new RectF(1, 2, 0, 0) },
				new object[] { "1,  2, 0, 0", true, new RectF(1, 2, 0, 0) },

				new object[] { "0.0,0.0, 0, 0", true, new RectF(0, 0, 0, 0) },
				new object[] { "0.0, 0.0, 0, 0", true, new RectF(0, 0, 0, 0) },
				new object[] { "0.0,  0.0, 0, 0", true, new RectF(0, 0, 0, 0) },

				new object[] { "1.1,2.1, 0, 0", true, new RectF(1.1f, 2.1f, 0, 0) },
				new object[] { "1.1, 2.1, 0, 0", true, new RectF(1.1f, 2.1f, 0, 0) },
				new object[] { "1.1,  2.1, 0, 0", true, new RectF(1.1f, 2.1f, 0, 0) },

				new object[] { "0,-0, 0, 0", true, new RectF(0, 0, 0, 0) },
				new object[] { "-0, 0, 0, 0", true, new RectF(0, 0, 0, 0) },
				new object[] { "-0,  -0, 0, 0", true, new RectF(0, 0, 0, 0) },

				new object[] { "-1,2, 0, 0", true, new RectF(-1, 2, 0, 0) },
				new object[] { "1, -2, 0, 0", true, new RectF(1, -2, 0, 0) },
				new object[] { "-1,  -2, 0, 0", true, new RectF(-1, -2, 0, 0) },

				new object[] { "-0.0,0.0, 0, 0", true, new RectF(0, 0, 0, 0) },
				new object[] { "0.0, -0.0, 0, 0", true, new RectF(0, 0, 0, 0) },
				new object[] { "-0.0,  -0.0, 0, 0", true, new RectF(0, 0, 0, 0) },

				new object[] { "-1.1,2.1, 0, 0", true, new RectF(-1.1f, 2.1f, 0, 0) },
				new object[] { "1.1, -2.1, 0, 0", true, new RectF(1.1f, -2.1f, 0, 0) },
				new object[] { "-1.1,  -2.1, 0, 0", true, new RectF(-1.1f, -2.1f, 0, 0) },

				new object[] { ".1,0, 0, 0", true, new RectF(0.1f, 0, 0, 0) },
				new object[] { "-.1, 1, 0, 0", true, new RectF(-0.1f, 1, 0, 0) },
				new object[] { "1, 	 1, 0, 0", true, new RectF(1, 1, 0, 0) },

				new object[] { "0", false, new RectF(0, 0, 0, 0) },
				new object[] { ",1, 0, 0", false, new RectF(0, 1, 0, 0) },
				new object[] { "1,, 0, 0", false, new RectF(1, 0, 0, 0) },
				new object[] { "", false, new RectF(0, 0, 0, 0) },
			};
	}
}
