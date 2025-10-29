using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class I8 : ContentPage
	{
		public long l0 { get; set; }
		public long l1 { get; set; }
		public long l2 { get; set; }
		public long l3 { get; set; }
		public long l4 { get; set; }
		public long l5 { get; set; }
		public long l6 { get; set; }
		public long l7 { get; set; }
		public long l8 { get; set; }
		public long l9 { get; set; }
		public ulong ul0 { get; set; }
		public ulong ul1 { get; set; }
		public ulong ul2 { get; set; }
		public ulong ul3 { get; set; }
		public ulong ul4 { get; set; }
		public ulong ul5 { get; set; }

		public I8()
		{
			InitializeComponent();
		}

		public I8(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void I8AreConverted(bool useCompiledXaml)
			{
				var p = new I8(useCompiledXaml);
				Assert.Equal(0L, p.l0);
				Assert.Equal((long)int.MaxValue, p.l1);
				Assert.Equal((long)uint.MaxValue, p.l2);
				Assert.Equal(long.MaxValue, p.l3);
				Assert.Equal((long)-int.MaxValue, p.l4);
				Assert.Equal((long)-uint.MaxValue, p.l5);
				Assert.Equal(-long.MaxValue, p.l6);
				Assert.Equal((long)256, p.l7);
				Assert.Equal((long)-256, p.l8);
				Assert.Equal((long)127, p.l9);
				Assert.Equal(0UL, p.ul0);
				Assert.Equal((ulong)int.MaxValue, p.ul1);
				Assert.Equal((ulong)uint.MaxValue, p.ul2);
				Assert.Equal((ulong)long.MaxValue, p.ul3);
				Assert.Equal(ulong.MaxValue, p.ul4);
				Assert.Equal((ulong)256, p.ul5);
			}
		}
	}
}