using Microsoft.Maui.Graphics.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Graphics.Tests
{
	public class PointTypeConverterTests
	{

		[Theory]
		[MemberData(nameof(PointConvertData))]
		public void ConvertsPointFromString(string from, bool expectedSuccess, Point expectedResult)
		{
			var ok = from.TryConvertFrom<PointTypeConverter, Point>(out var p);
			Assert.Equal(expectedSuccess, ok);

			if (expectedSuccess)
				Assert.Equal(expectedResult, p);
		}

		[Theory]
		[MemberData(nameof(PointFConvertData))]
		public void ConvertsPointFFromString(string from, bool expectedSuccess, PointF expectedResult)
		{
			var ok = from.TryConvertFrom<PointFTypeConverter, PointF>(out var p);
			Assert.Equal(expectedSuccess, ok);

			if (expectedSuccess)
				Assert.Equal(expectedResult, p);
		}

		// 0, 0
		public static IEnumerable<object[]> PointConvertData()
			=> new List<object[]>
			{
				new object[] { "0,0", true, new Point(0, 0) },
				new object[] { "0, 0", true, new Point(0, 0) },
				new object[] { "0,  0", true, new Point(0, 0) },

				new object[] { "1,2", true, new Point(1, 2) },
				new object[] { "1, 2", true, new Point(1, 2) },
				new object[] { "1,  2", true, new Point(1, 2) },

				new object[] { "0.0,0.0", true, new Point(0, 0) },
				new object[] { "0.0, 0.0", true, new Point(0, 0) },
				new object[] { "0.0,  0.0", true, new Point(0, 0) },

				new object[] { "1.1,2.1", true, new Point(1.1, 2.1) },
				new object[] { "1.1, 2.1", true, new Point(1.1, 2.1) },
				new object[] { "1.1,  2.1", true, new Point(1.1, 2.1) },

				new object[] { "0,-0", true, new Point(0, 0) },
				new object[] { "-0, 0", true, new Point(0, 0) },
				new object[] { "-0,  -0", true, new Point(0, 0) },

				new object[] { "-1,2", true, new Point(-1, 2) },
				new object[] { "1, -2", true, new Point(1, -2) },
				new object[] { "-1,  -2", true, new Point(-1, -2) },

				new object[] { "-0.0,0.0", true, new Point(0, 0) },
				new object[] { "0.0, -0.0", true, new Point(0, 0) },
				new object[] { "-0.0,  -0.0", true, new Point(0, 0) },

				new object[] { "-1.1,2.1", true, new Point(-1.1, 2.1) },
				new object[] { "1.1, -2.1", true, new Point(1.1, -2.1) },
				new object[] { "-1.1,  -2.1", true, new Point(-1.1, -2.1) },

				new object[] { ".1,0", true, new Point(0.1, 0) },
				new object[] { "-.1, 1", true, new Point(-0.1, 1) },
				new object[] { "1, 	 1", true, new Point(1, 1) },

				new object[] { "0", false, new Point(0, 0) },
				new object[] { ",1", false, new Point(0, 1) },
				new object[] { "1,", false, new Point(1, 0) },
				new object[] { "", false, new Point(0, 0) },
			};


		public static IEnumerable<object[]> PointFConvertData()
			=> new List<object[]>
			{
				new object[] { "0,0", true, new PointF(0, 0) },
				new object[] { "0, 0", true, new PointF(0, 0) },
				new object[] { "0,  0", true, new PointF(0, 0) },

				new object[] { "1,2", true, new PointF(1, 2) },
				new object[] { "1, 2", true, new PointF(1, 2) },
				new object[] { "1,  2", true, new PointF(1, 2) },

				new object[] { "0.0,0.0", true, new PointF(0, 0) },
				new object[] { "0.0, 0.0", true, new PointF(0, 0) },
				new object[] { "0.0,  0.0", true, new PointF(0, 0) },

				new object[] { "1.1,2.1", true, new PointF(1.1f, 2.1f) },
				new object[] { "1.1, 2.1", true, new PointF(1.1f, 2.1f) },
				new object[] { "1.1,  2.1", true, new PointF(1.1f, 2.1f) },

				new object[] { "0,-0", true, new PointF(0, 0) },
				new object[] { "-0, 0", true, new PointF(0, 0) },
				new object[] { "-0,  -0", true, new PointF(0, 0) },

				new object[] { "-1,2", true, new PointF(-1, 2) },
				new object[] { "1, -2", true, new PointF(1, -2) },
				new object[] { "-1,  -2", true, new PointF(-1, -2) },

				new object[] { "-0.0,0.0", true, new PointF(0, 0) },
				new object[] { "0.0, -0.0", true, new PointF(0, 0) },
				new object[] { "-0.0,  -0.0", true, new PointF(0, 0) },

				new object[] { "-1.1,2.1", true, new PointF(-1.1f, 2.1f) },
				new object[] { "1.1, -2.1", true, new PointF(1.1f, -2.1f) },
				new object[] { "-1.1,  -2.1", true, new PointF(-1.1f, -2.1f) },

				new object[] { ".1,0", true, new PointF(0.1f, 0) },
				new object[] { "-.1, 1", true, new PointF(-0.1f, 1) },
				new object[] { "1, 	 1", true, new PointF(1, 1) },

				new object[] { "0", false, new PointF(0, 0) },
				new object[] { ",1", false, new PointF(0, 1) },
				new object[] { "1,", false, new PointF(1, 0) },
				new object[] { "", false, new PointF(0, 0) },
			};
	}
}
