using Microsoft.Maui.Graphics.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Graphics.Tests
{
	public class SizeTypeConverterTests
	{

		[Theory]
		[MemberData(nameof(SizeConvertData))]
		public void ConvertsSizeFromString(string from, bool expectedSuccess, Size expectedResult)
		{
			var ok = from.TryConvertFrom<SizeTypeConverter, Size>(out var p);
			Assert.Equal(expectedSuccess, ok);

			if (expectedSuccess)
				Assert.Equal(expectedResult, p);
		}

		[Theory]
		[MemberData(nameof(SizeFConvertData))]
		public void ConvertsSizeFFromString(string from, bool expectedSuccess, SizeF expectedResult)
		{
			var ok = from.TryConvertFrom<SizeFTypeConverter, SizeF>(out var p); 
			Assert.Equal(expectedSuccess, ok);

			if (expectedSuccess)
				Assert.Equal(expectedResult, p);
		}

		// 0, 0
		public static IEnumerable<object[]> SizeConvertData()
			=> new List<object[]>
			{
				new object[] { "0,0", true, new Size(0, 0) },
				new object[] { "0, 0", true, new Size(0, 0) },
				new object[] { "0,  0", true, new Size(0, 0) },

				new object[] { "1,2", true, new Size(1, 2) },
				new object[] { "1, 2", true, new Size(1, 2) },
				new object[] { "1,  2", true, new Size(1, 2) },

				new object[] { "0.0,0.0", true, new Size(0, 0) },
				new object[] { "0.0, 0.0", true, new Size(0, 0) },
				new object[] { "0.0,  0.0", true, new Size(0, 0) },

				new object[] { "1.1,2.1", true, new Size(1.1, 2.1) },
				new object[] { "1.1, 2.1", true, new Size(1.1, 2.1) },
				new object[] { "1.1,  2.1", true, new Size(1.1, 2.1) },

				new object[] { "0,-0", true, new Size(0, 0) },
				new object[] { "-0, 0", true, new Size(0, 0) },
				new object[] { "-0,  -0", true, new Size(0, 0) },

				new object[] { "-1,2", true, new Size(-1, 2) },
				new object[] { "1, -2", true, new Size(1, -2) },
				new object[] { "-1,  -2", true, new Size(-1, -2) },

				new object[] { "-0.0,0.0", true, new Size(0, 0) },
				new object[] { "0.0, -0.0", true, new Size(0, 0) },
				new object[] { "-0.0,  -0.0", true, new Size(0, 0) },

				new object[] { "-1.1,2.1", true, new Size(-1.1, 2.1) },
				new object[] { "1.1, -2.1", true, new Size(1.1, -2.1) },
				new object[] { "-1.1,  -2.1", true, new Size(-1.1, -2.1) },

				new object[] { ".1,0", true, new Size(0.1, 0) },
				new object[] { "-.1, 1", true, new Size(-0.1, 1) },
				new object[] { "1, 	 1", true, new Size(1, 1) },

				new object[] { "0", false, new Size(0, 0) },
				new object[] { ",1", false, new Size(0, 1) },
				new object[] { "1,", false, new Size(1, 0) },
				new object[] { "", false, new Size(0, 0) },
			};


		public static IEnumerable<object[]> SizeFConvertData()
			=> new List<object[]>
			{
				new object[] { "0,0", true, new SizeF(0, 0) },
				new object[] { "0, 0", true, new SizeF(0, 0) },
				new object[] { "0,  0", true, new SizeF(0, 0) },

				new object[] { "1,2", true, new SizeF(1, 2) },
				new object[] { "1, 2", true, new SizeF(1, 2) },
				new object[] { "1,  2", true, new SizeF(1, 2) },

				new object[] { "0.0,0.0", true, new SizeF(0, 0) },
				new object[] { "0.0, 0.0", true, new SizeF(0, 0) },
				new object[] { "0.0,  0.0", true, new SizeF(0, 0) },

				new object[] { "1.1,2.1", true, new SizeF(1.1f, 2.1f) },
				new object[] { "1.1, 2.1", true, new SizeF(1.1f, 2.1f) },
				new object[] { "1.1,  2.1", true, new SizeF(1.1f, 2.1f) },

				new object[] { "0,-0", true, new SizeF(0, 0) },
				new object[] { "-0, 0", true, new SizeF(0, 0) },
				new object[] { "-0,  -0", true, new SizeF(0, 0) },

				new object[] { "-1,2", true, new SizeF(-1, 2) },
				new object[] { "1, -2", true, new SizeF(1, -2) },
				new object[] { "-1,  -2", true, new SizeF(-1, -2) },

				new object[] { "-0.0,0.0", true, new SizeF(0, 0) },
				new object[] { "0.0, -0.0", true, new SizeF(0, 0) },
				new object[] { "-0.0,  -0.0", true, new SizeF(0, 0) },

				new object[] { "-1.1,2.1", true, new SizeF(-1.1f, 2.1f) },
				new object[] { "1.1, -2.1", true, new SizeF(1.1f, -2.1f) },
				new object[] { "-1.1,  -2.1", true, new SizeF(-1.1f, -2.1f) },

				new object[] { ".1,0", true, new SizeF(0.1f, 0) },
				new object[] { "-.1, 1", true, new SizeF(-0.1f, 1) },
				new object[] { "1, 	 1", true, new SizeF(1, 1) },

				new object[] { "0", false, new SizeF(0, 0) },
				new object[] { ",1", false, new SizeF(0, 1) },
				new object[] { "1,", false, new SizeF(1, 0) },
				new object[] { "", false, new SizeF(0, 0) },
			};
	}
}
