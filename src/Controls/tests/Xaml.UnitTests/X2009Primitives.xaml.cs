using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class X2009Primitives : ContentPage
	{
		public X2009Primitives()
		{
			InitializeComponent();
		}

		public X2009Primitives(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void SupportsXString(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);
				Assert.True(layout.Resources.ContainsKey("aString"));
				Assert.Equal("foobar", layout.Resources["aString"]);

				Assert.True(layout.Resources.ContainsKey("defaultString"));
				Assert.Equal(String.Empty, layout.Resources["defaultString"]);

			}

			[InlineData(false)]
			[InlineData(true)]
			public void SupportsXObject(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);
				Assert.True(layout.Resources.ContainsKey("syncHandle"));
				var value = layout.Resources["syncHandle"];
				Assert.NotNull(value);
				Assert.IsType<object>(value);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void SupportsXBoolean(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);

				Assert.True(layout.Resources.ContainsKey("falsebool"));
				var falsebool = layout.Resources["falsebool"];
				Assert.NotNull(falsebool);
				Assert.IsType<bool>(falsebool);
				Assert.Equal(false, (bool)falsebool);

				Assert.True(layout.Resources.ContainsKey("truebool"));
				var truebool = layout.Resources["truebool"];
				Assert.NotNull(truebool);
				Assert.IsType<bool>(truebool);
				Assert.Equal(true, (bool)truebool);

				Assert.True(layout.Resources.ContainsKey("defaultbool"));
				var defaultbool = layout.Resources["defaultbool"];
				Assert.NotNull(defaultbool);
				Assert.IsType<bool>(defaultbool);
				Assert.Equal(default(bool), (bool)defaultbool);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void SupportsXChar(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);

				Assert.True(layout.Resources.ContainsKey("singleChar"));
				var singleChar = layout.Resources["singleChar"];
				Assert.NotNull(singleChar);
				Assert.IsType<char>(singleChar);
				Assert.Equal('f', (char)singleChar);

				Assert.True(layout.Resources.ContainsKey("multipleChar"));
				var multipleChar = layout.Resources["multipleChar"];
				Assert.NotNull(multipleChar);
				Assert.IsType<char>(multipleChar);
				Assert.Equal(default(char), (char)multipleChar);

				Assert.True(layout.Resources.ContainsKey("defaultChar"));
				var defaultChar = layout.Resources["defaultChar"];
				Assert.NotNull(defaultChar);
				Assert.IsType<char>(defaultChar);
				Assert.Equal(default(char), (char)defaultChar);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void SupportsXNumbers(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);

				Assert.True(layout.Resources.ContainsKey("aDecimal"));
				var aDecimal = layout.Resources["aDecimal"];
				Assert.NotNull(aDecimal);
				Assert.IsType<decimal>(aDecimal);
				Assert.Equal(1000, (decimal)aDecimal);

				Assert.True(layout.Resources.ContainsKey("defaultDecimal"));
				var defaultDecimal = layout.Resources["defaultDecimal"];
				Assert.NotNull(defaultDecimal);
				Assert.IsType<decimal>(defaultDecimal);
				Assert.Equal(default(decimal), (decimal)defaultDecimal);

				Assert.True(layout.Resources.ContainsKey("aSingle"));
				var aSingle = layout.Resources["aSingle"];
				Assert.NotNull(aSingle);
				Assert.IsType<float>(aSingle);
				Assert.Equal(42.2f, (float)aSingle, .0001f);

				Assert.True(layout.Resources.ContainsKey("defaultSingle"));
				var defaultSingle = layout.Resources["defaultSingle"];
				Assert.NotNull(defaultSingle);
				Assert.IsType<Single>(defaultSingle);
				Assert.Equal(default(float), (float)defaultSingle, .0001f);

				Assert.True(layout.Resources.ContainsKey("aDouble"));
				var aDouble = layout.Resources["aDouble"];
				Assert.NotNull(aDouble);
				Assert.IsType<double>(aDouble);
				Assert.Equal(42.3d, (double)aDouble, .0001d);

				Assert.True(layout.Resources.ContainsKey("aNegativeDouble"));
				var aNegativeDouble = layout.Resources["aNegativeDouble"];
				Assert.NotNull(aNegativeDouble);
				Assert.IsType<double>(aNegativeDouble);
				Assert.Equal(-42.3d, (double)aNegativeDouble, .0001d);

				Assert.True(layout.Resources.ContainsKey("defaultDouble"));
				var defaultDouble = layout.Resources["defaultDouble"];
				Assert.NotNull(defaultDouble);
				Assert.IsType<double>(defaultDouble);
				Assert.Equal(default(double), (double)defaultDouble, .0001d);

				Assert.True(layout.Resources.ContainsKey("aByte"));
				var aByte = layout.Resources["aByte"];
				Assert.NotNull(aByte);
				Assert.IsType<byte>(aByte);
				Assert.Equal(54, (byte)aByte);

				Assert.True(layout.Resources.ContainsKey("aSByte"));
				var aSByte = layout.Resources["aSByte"];
				Assert.NotNull(aSByte);
				Assert.IsType<sbyte>(aSByte);
				Assert.Equal(42, (sbyte)aSByte);

				Assert.True(layout.Resources.ContainsKey("defaultByte"));
				var defaultByte = layout.Resources["defaultByte"];
				Assert.NotNull(defaultByte);
				Assert.IsType<byte>(defaultByte);
				Assert.Equal(default(byte), (byte)defaultByte);

				Assert.True(layout.Resources.ContainsKey("anInt16"));
				var anInt16 = layout.Resources["anInt16"];
				Assert.NotNull(anInt16);
				Assert.IsType<short>(anInt16);
				Assert.Equal(43, (short)anInt16);

				Assert.True(layout.Resources.ContainsKey("aUInt16"));
				var aUInt16 = layout.Resources["aUInt16"];
				Assert.NotNull(aUInt16);
				Assert.IsType<ushort>(aUInt16);
				Assert.Equal(43, (ushort)aUInt16);

				Assert.True(layout.Resources.ContainsKey("defaultInt16"));
				var defaultInt16 = layout.Resources["defaultInt16"];
				Assert.NotNull(defaultInt16);
				Assert.IsType<short>(defaultInt16);
				Assert.Equal(default(short), (short)defaultInt16);

				Assert.True(layout.Resources.ContainsKey("anInt32"));
				var anInt32 = layout.Resources["anInt32"];
				Assert.NotNull(anInt32);
				Assert.IsType<int>(anInt32);
				Assert.Equal(44, (int)anInt32);

				Assert.True(layout.Resources.ContainsKey("aUInt32"));
				var aUInt32 = layout.Resources["aUInt32"];
				Assert.NotNull(aUInt32);
				Assert.IsType<uint>(aUInt32);
				Assert.Equal(44, (uint)aUInt32);

				Assert.True(layout.Resources.ContainsKey("defaultInt32"));
				var defaultInt32 = layout.Resources["defaultInt32"];
				Assert.NotNull(defaultInt32);
				Assert.IsType<int>(defaultInt32);
				Assert.Equal(default(int), (int)defaultInt32);

				Assert.True(layout.Resources.ContainsKey("anInt64"));
				var anInt64 = layout.Resources["anInt64"];
				Assert.NotNull(anInt64);
				Assert.IsType<long>(anInt64);
				Assert.Equal(45, (long)anInt64);

				Assert.True(layout.Resources.ContainsKey("aUInt64"));
				var aUInt64 = layout.Resources["aUInt64"];
				Assert.NotNull(aUInt64);
				Assert.IsType<ulong>(aUInt64);
				Assert.Equal(45, (ulong)aUInt64);

				Assert.True(layout.Resources.ContainsKey("defaultInt64"));
				var defaultInt64 = layout.Resources["defaultInt64"];
				Assert.NotNull(defaultInt64);
				Assert.IsType<long>(defaultInt64);
				Assert.Equal(default(long), (long)defaultInt64);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void SupportsXTimeSpan(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);

				Assert.True(layout.Resources.ContainsKey("aTimeSpan"));
				var aTimeSpan = layout.Resources["aTimeSpan"];
				Assert.NotNull(aTimeSpan);
				Assert.IsType<TimeSpan>(aTimeSpan);
				Assert.Equal(new TimeSpan(6, 12, 14, 45, 344).Add(TimeSpan.FromTicks(8000)), (TimeSpan)aTimeSpan);

				Assert.True(layout.Resources.ContainsKey("defaultTimeSpan"));
				var defaultTimeSpan = layout.Resources["defaultTimeSpan"];
				Assert.NotNull(defaultTimeSpan);
				Assert.IsType<TimeSpan>(defaultTimeSpan);
				Assert.Equal(default(TimeSpan), (TimeSpan)defaultTimeSpan);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void SupportsXUri(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);

				Assert.True(layout.Resources.ContainsKey("anUri"));
				var anUri = layout.Resources["anUri"];
				Assert.NotNull(anUri);
				Assert.IsType<Uri>(anUri);
				Assert.Equal(new Uri("http://xamarin.com/forms"), (Uri)anUri);

				Assert.True(layout.Resources.ContainsKey("defaultUri"));
				var defaultUri = layout.Resources["defaultUri"];
				Assert.Null(defaultUri);
			}
		}
	}
}