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
		}
		public class Tests
		{
			[Theory]
			[InlineData(true)]
			public void SupportsXString(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);
				Assert.True(layout.Resources.ContainsKey("aString"));
				Assert.Equal("foobar", layout.Resources["aString"]);

				Assert.True(layout.Resources.ContainsKey("defaultString"));
				Assert.Equal(String.Empty, layout.Resources["defaultString"]);

			}

			[Theory]
			[InlineData(true)]
			public void SupportsXObject(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);
				Assert.True(layout.Resources.ContainsKey("syncHandle"));
				var value = layout.Resources["syncHandle"];
				Assert.NotNull(value);
				Assert.That(value, Is.TypeOf<object>());
			}

			[Theory]
			[InlineData(true)]
			public void SupportsXBoolean(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);

				Assert.True(layout.Resources.ContainsKey("falsebool"));
				var falsebool = layout.Resources["falsebool"];
				Assert.NotNull(falsebool);
				Assert.That(falsebool, Is.TypeOf<bool>());
				Assert.Equal(false, (bool)falsebool);

				Assert.True(layout.Resources.ContainsKey("truebool"));
				var truebool = layout.Resources["truebool"];
				Assert.NotNull(truebool);
				Assert.That(truebool, Is.TypeOf<bool>());
				Assert.Equal(true, (bool)truebool);

				Assert.True(layout.Resources.ContainsKey("defaultbool"));
				var defaultbool = layout.Resources["defaultbool"];
				Assert.NotNull(defaultbool);
				Assert.That(defaultbool, Is.TypeOf<bool>());
				Assert.Equal(default(bool), (bool)defaultbool);
			}

			[Theory]
			[InlineData(true)]
			public void SupportsXChar(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);

				Assert.True(layout.Resources.ContainsKey("singleChar"));
				var singleChar = layout.Resources["singleChar"];
				Assert.NotNull(singleChar);
				Assert.That(singleChar, Is.TypeOf<char>());
				Assert.Equal('f', (char)singleChar);

				Assert.True(layout.Resources.ContainsKey("multipleChar"));
				var multipleChar = layout.Resources["multipleChar"];
				Assert.NotNull(multipleChar);
				Assert.That(multipleChar, Is.TypeOf<char>());
				Assert.Equal(default(char), (char)multipleChar);

				Assert.True(layout.Resources.ContainsKey("defaultChar"));
				var defaultChar = layout.Resources["defaultChar"];
				Assert.NotNull(defaultChar);
				Assert.That(defaultChar, Is.TypeOf<char>());
				Assert.Equal(default(char), (char)defaultChar);
			}

			[Theory]
			[InlineData(true)]
			public void SupportsXNumbers(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);

				Assert.True(layout.Resources.ContainsKey("aDecimal"));
				var aDecimal = layout.Resources["aDecimal"];
				Assert.NotNull(aDecimal);
				Assert.That(aDecimal, Is.TypeOf<decimal>());
				Assert.Equal(1000, (decimal)aDecimal);

				Assert.True(layout.Resources.ContainsKey("defaultDecimal"));
				var defaultDecimal = layout.Resources["defaultDecimal"];
				Assert.NotNull(defaultDecimal);
				Assert.That(defaultDecimal, Is.TypeOf<decimal>());
				Assert.Equal(default(decimal), (decimal)defaultDecimal);

				Assert.True(layout.Resources.ContainsKey("aSingle"));
				var aSingle = layout.Resources["aSingle"];
				Assert.NotNull(aSingle);
				Assert.That(aSingle, Is.TypeOf<float>());
				Assert.Equal(42.2f, (float)aSingle, .0001f);

				Assert.True(layout.Resources.ContainsKey("defaultSingle"));
				var defaultSingle = layout.Resources["defaultSingle"];
				Assert.NotNull(defaultSingle);
				Assert.That(defaultSingle, Is.TypeOf<Single>());
				Assert.Equal(default(float), (float)defaultSingle, .0001f);

				Assert.True(layout.Resources.ContainsKey("aDouble"));
				var aDouble = layout.Resources["aDouble"];
				Assert.NotNull(aDouble);
				Assert.That(aDouble, Is.TypeOf<double>());
				Assert.Equal(42.3d, (double)aDouble, .0001d);

				Assert.True(layout.Resources.ContainsKey("aNegativeDouble"));
				var aNegativeDouble = layout.Resources["aNegativeDouble"];
				Assert.NotNull(aNegativeDouble);
				Assert.That(aNegativeDouble, Is.TypeOf<double>());
				Assert.Equal(-42.3d, (double)aNegativeDouble, .0001d);

				Assert.True(layout.Resources.ContainsKey("defaultDouble"));
				var defaultDouble = layout.Resources["defaultDouble"];
				Assert.NotNull(defaultDouble);
				Assert.That(defaultDouble, Is.TypeOf<double>());
				Assert.Equal(default(double), (double)defaultDouble, .0001d);

				Assert.True(layout.Resources.ContainsKey("aByte"));
				var aByte = layout.Resources["aByte"];
				Assert.NotNull(aByte);
				Assert.That(aByte, Is.TypeOf<byte>());
				Assert.Equal(54, (byte)aByte);

				Assert.True(layout.Resources.ContainsKey("aSByte"));
				var aSByte = layout.Resources["aSByte"];
				Assert.NotNull(aSByte);
				Assert.That(aSByte, Is.TypeOf<sbyte>());
				Assert.Equal(42, (sbyte)aSByte);

				Assert.True(layout.Resources.ContainsKey("defaultByte"));
				var defaultByte = layout.Resources["defaultByte"];
				Assert.NotNull(defaultByte);
				Assert.That(defaultByte, Is.TypeOf<byte>());
				Assert.Equal(default(byte), (byte)defaultByte);

				Assert.True(layout.Resources.ContainsKey("anInt16"));
				var anInt16 = layout.Resources["anInt16"];
				Assert.NotNull(anInt16);
				Assert.That(anInt16, Is.TypeOf<short>());
				Assert.Equal(43, (short)anInt16);

				Assert.True(layout.Resources.ContainsKey("aUInt16"));
				var aUInt16 = layout.Resources["aUInt16"];
				Assert.NotNull(aUInt16);
				Assert.That(aUInt16, Is.TypeOf<ushort>());
				Assert.Equal(43, (ushort)aUInt16);

				Assert.True(layout.Resources.ContainsKey("defaultInt16"));
				var defaultInt16 = layout.Resources["defaultInt16"];
				Assert.NotNull(defaultInt16);
				Assert.That(defaultInt16, Is.TypeOf<short>());
				Assert.Equal(default(short), (short)defaultInt16);

				Assert.True(layout.Resources.ContainsKey("anInt32"));
				var anInt32 = layout.Resources["anInt32"];
				Assert.NotNull(anInt32);
				Assert.That(anInt32, Is.TypeOf<int>());
				Assert.Equal(44, (int)anInt32);

				Assert.True(layout.Resources.ContainsKey("aUInt32"));
				var aUInt32 = layout.Resources["aUInt32"];
				Assert.NotNull(aUInt32);
				Assert.That(aUInt32, Is.TypeOf<uint>());
				Assert.Equal(44, (uint)aUInt32);

				Assert.True(layout.Resources.ContainsKey("defaultInt32"));
				var defaultInt32 = layout.Resources["defaultInt32"];
				Assert.NotNull(defaultInt32);
				Assert.That(defaultInt32, Is.TypeOf<int>());
				Assert.Equal(default(int), (int)defaultInt32);

				Assert.True(layout.Resources.ContainsKey("anInt64"));
				var anInt64 = layout.Resources["anInt64"];
				Assert.NotNull(anInt64);
				Assert.That(anInt64, Is.TypeOf<long>());
				Assert.Equal(45, (long)anInt64);

				Assert.True(layout.Resources.ContainsKey("aUInt64"));
				var aUInt64 = layout.Resources["aUInt64"];
				Assert.NotNull(aUInt64);
				Assert.That(aUInt64, Is.TypeOf<ulong>());
				Assert.Equal(45, (ulong)aUInt64);

				Assert.True(layout.Resources.ContainsKey("defaultInt64"));
				var defaultInt64 = layout.Resources["defaultInt64"];
				Assert.NotNull(defaultInt64);
				Assert.That(defaultInt64, Is.TypeOf<long>());
				Assert.Equal(default(long), (long)defaultInt64);
			}

			[Theory]
			[InlineData(true)]
			public void SupportsXTimeSpan(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);

				Assert.True(layout.Resources.ContainsKey("aTimeSpan"));
				var aTimeSpan = layout.Resources["aTimeSpan"];
				Assert.NotNull(aTimeSpan);
				Assert.That(aTimeSpan, Is.TypeOf<TimeSpan>());
				Assert.Equal(new TimeSpan(6, 12, 14, 45, 344).Add(TimeSpan.FromTicks(8000)), (TimeSpan)aTimeSpan);

				Assert.True(layout.Resources.ContainsKey("defaultTimeSpan"));
				var defaultTimeSpan = layout.Resources["defaultTimeSpan"];
				Assert.NotNull(defaultTimeSpan);
				Assert.That(defaultTimeSpan, Is.TypeOf<TimeSpan>());
				Assert.Equal(default(TimeSpan), (TimeSpan)defaultTimeSpan);
			}

			[Theory]
			[InlineData(true)]
			public void SupportsXUri(bool useCompiledXaml)
			{
				var layout = new X2009Primitives(useCompiledXaml);

				Assert.True(layout.Resources.ContainsKey("anUri"));
				var anUri = layout.Resources["anUri"];
				Assert.NotNull(anUri);
				Assert.That(anUri, Is.TypeOf<Uri>());
				Assert.Equal(new Uri("http://xamarin.com/forms"), (Uri)anUri);

				Assert.True(layout.Resources.ContainsKey("defaultUri"));
				var defaultUri = layout.Resources["defaultUri"];
				Assert.Null(defaultUri);
			}
		}
	}
}