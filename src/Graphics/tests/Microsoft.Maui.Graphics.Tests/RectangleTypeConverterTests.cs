using Microsoft.Maui.Graphics.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Graphics.Tests
{
	public class RectangleTypeConverterTests
	{

		[Theory]
		[MemberData(nameof(RectangleConvertData))]
		public void ConvertsRectangleFromString(string from, bool expectedSuccess, Rectangle expectedResult)
		{
			var ok = from.TryConvertFrom<RectangleTypeConverter, Rectangle>(out var p);
			Assert.Equal(expectedSuccess, ok);

			if (expectedSuccess)
				Assert.Equal(expectedResult, p);
		}

		[Theory]
		[MemberData(nameof(RectangleFConvertData))]
		public void ConvertsRectangleFFromString(string from, bool expectedSuccess, RectangleF expectedResult)
		{
			var ok = from.TryConvertFrom<RectangleFTypeConverter, RectangleF>(out var p);
			Assert.Equal(expectedSuccess, ok);

			if (expectedSuccess)
				Assert.Equal(expectedResult, p);
		}

		// 0, 0
		public static IEnumerable<object[]> RectangleConvertData()
			=> new List<object[]>
			{
				new object[] { "1,2,3,4", true, new Rectangle(1, 2, 3, 4) },
				new object[] { "1.1,2.0,3,4.4", true, new Rectangle(1.1, 2, 3, 4.4) },

				new object[] { "0,0, 0, 0", true, new Rectangle(0, 0, 0, 0) },
				new object[] { "0, 0, 0, 0", true, new Rectangle(0, 0, 0, 0) },
				new object[] { "0,  0, 0, 0", true, new Rectangle(0, 0, 0, 0) },

				new object[] { "1,2, 0, 0", true, new Rectangle(1, 2, 0, 0) },
				new object[] { "1, 2, 0, 0", true, new Rectangle(1, 2, 0, 0) },
				new object[] { "1,  2, 0, 0", true, new Rectangle(1, 2, 0, 0) },

				new object[] { "0.0,0.0, 0, 0", true, new Rectangle(0, 0, 0, 0) },
				new object[] { "0.0, 0.0, 0, 0", true, new Rectangle(0, 0, 0, 0) },
				new object[] { "0.0,  0.0, 0, 0", true, new Rectangle(0, 0, 0, 0) },

				new object[] { "1.1,2.1, 0, 0", true, new Rectangle(1.1, 2.1, 0, 0) },
				new object[] { "1.1, 2.1, 0, 0", true, new Rectangle(1.1, 2.1, 0, 0) },
				new object[] { "1.1,  2.1, 0, 0", true, new Rectangle(1.1, 2.1, 0, 0) },

				new object[] { "0,-0, 0, 0", true, new Rectangle(0, 0, 0, 0) },
				new object[] { "-0, 0, 0, 0", true, new Rectangle(0, 0, 0, 0) },
				new object[] { "-0,  -0, 0, 0", true, new Rectangle(0, 0, 0, 0) },

				new object[] { "-1,2, 0, 0", true, new Rectangle(-1, 2, 0, 0) },
				new object[] { "1, -2, 0, 0", true, new Rectangle(1, -2, 0, 0) },
				new object[] { "-1,  -2, 0, 0", true, new Rectangle(-1, -2, 0, 0) },

				new object[] { "-0.0,0.0, 0, 0", true, new Rectangle(0, 0, 0, 0) },
				new object[] { "0.0, -0.0, 0, 0", true, new Rectangle(0, 0, 0, 0) },
				new object[] { "-0.0,  -0.0, 0, 0", true, new Rectangle(0, 0, 0, 0) },

				new object[] { "-1.1,2.1, 0, 0", true, new Rectangle(-1.1, 2.1, 0, 0) },
				new object[] { "1.1, -2.1, 0, 0", true, new Rectangle(1.1, -2.1, 0, 0) },
				new object[] { "-1.1,  -2.1, 0, 0", true, new Rectangle(-1.1, -2.1, 0, 0) },

				new object[] { ".1,0, 0, 0", true, new Rectangle(0.1, 0, 0, 0) },
				new object[] { "-.1, 1, 0, 0", true, new Rectangle(-0.1, 1, 0, 0) },
				new object[] { "1, 	 1, 0, 0", true, new Rectangle(1, 1, 0, 0) },

				new object[] { "0", false, new Rectangle(0, 0, 0, 0) },
				new object[] { ",1, 0, 0", false, new Rectangle(0, 1, 0, 0) },
				new object[] { "1,, 0, 0", false, new Rectangle(1, 0, 0, 0) },
				new object[] { "", false, new Rectangle(0, 0, 0, 0) },
			};


		public static IEnumerable<object[]> RectangleFConvertData()
			=> new List<object[]>
			{
				new object[] { "1,2,3,4", true, new RectangleF(1, 2, 3, 4) },
				new object[] { "1.1,2.0,3,4.4", true, new RectangleF(1.1f, 2, 3, 4.4f) },

				new object[] { "0,0, 0, 0", true, new RectangleF(0, 0, 0, 0) },
				new object[] { "0, 0, 0, 0", true, new RectangleF(0, 0, 0, 0) },
				new object[] { "0,  0, 0, 0", true, new RectangleF(0, 0, 0, 0) },

				new object[] { "1,2, 0, 0", true, new RectangleF(1, 2, 0, 0) },
				new object[] { "1, 2, 0, 0", true, new RectangleF(1, 2, 0, 0) },
				new object[] { "1,  2, 0, 0", true, new RectangleF(1, 2, 0, 0) },

				new object[] { "0.0,0.0, 0, 0", true, new RectangleF(0, 0, 0, 0) },
				new object[] { "0.0, 0.0, 0, 0", true, new RectangleF(0, 0, 0, 0) },
				new object[] { "0.0,  0.0, 0, 0", true, new RectangleF(0, 0, 0, 0) },

				new object[] { "1.1,2.1, 0, 0", true, new RectangleF(1.1f, 2.1f, 0, 0) },
				new object[] { "1.1, 2.1, 0, 0", true, new RectangleF(1.1f, 2.1f, 0, 0) },
				new object[] { "1.1,  2.1, 0, 0", true, new RectangleF(1.1f, 2.1f, 0, 0) },

				new object[] { "0,-0, 0, 0", true, new RectangleF(0, 0, 0, 0) },
				new object[] { "-0, 0, 0, 0", true, new RectangleF(0, 0, 0, 0) },
				new object[] { "-0,  -0, 0, 0", true, new RectangleF(0, 0, 0, 0) },

				new object[] { "-1,2, 0, 0", true, new RectangleF(-1, 2, 0, 0) },
				new object[] { "1, -2, 0, 0", true, new RectangleF(1, -2, 0, 0) },
				new object[] { "-1,  -2, 0, 0", true, new RectangleF(-1, -2, 0, 0) },

				new object[] { "-0.0,0.0, 0, 0", true, new RectangleF(0, 0, 0, 0) },
				new object[] { "0.0, -0.0, 0, 0", true, new RectangleF(0, 0, 0, 0) },
				new object[] { "-0.0,  -0.0, 0, 0", true, new RectangleF(0, 0, 0, 0) },

				new object[] { "-1.1,2.1, 0, 0", true, new RectangleF(-1.1f, 2.1f, 0, 0) },
				new object[] { "1.1, -2.1, 0, 0", true, new RectangleF(1.1f, -2.1f, 0, 0) },
				new object[] { "-1.1,  -2.1, 0, 0", true, new RectangleF(-1.1f, -2.1f, 0, 0) },

				new object[] { ".1,0, 0, 0", true, new RectangleF(0.1f, 0, 0, 0) },
				new object[] { "-.1, 1, 0, 0", true, new RectangleF(-0.1f, 1, 0, 0) },
				new object[] { "1, 	 1, 0, 0", true, new RectangleF(1, 1, 0, 0) },

				new object[] { "0", false, new RectangleF(0, 0, 0, 0) },
				new object[] { ",1, 0, 0", false, new RectangleF(0, 1, 0, 0) },
				new object[] { "1,, 0, 0", false, new RectangleF(1, 0, 0, 0) },
				new object[] { "", false, new RectangleF(0, 0, 0, 0) },
			};
	}
}
