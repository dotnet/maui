using System;
using Microsoft.Maui.Storage;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{

	[Category("Preferences")]
	[Collection("UsesPreferences")]
	public class Preferences_Tests
	{
		const string sharedNameTestData = "Shared";

		static DateTime testDateTime = new DateTime(2018, 05, 07);

		#region Static method tests
		[Theory]
		[InlineData("datetime1", null)]
		[InlineData("datetime1", sharedNameTestData)]
		public void Set_Get_DateTime(string key, string sharedName)
		{
			Preferences.Set(key, testDateTime, sharedName);

			Assert.Equal(testDateTime, Preferences.Get(key, DateTime.MinValue, sharedName));
		}

		[Theory]
		[InlineData("string1", "TEST", null)]
		[InlineData("string1", "TEST", sharedNameTestData)]
		public void Set_Get_String(string key, string value, string sharedName)
		{
			Preferences.Set(key, value, sharedName);

			Assert.Equal(value, Preferences.Get(key, null, sharedName));
		}

		[Theory]
		[InlineData("string1", "TEST", null)]
		[InlineData("string1", "TEST", sharedNameTestData)]
		public void Set_Set_Null_Get_String(string key, string value, string sharedName)
		{
			Preferences.Set(key, value, sharedName);
			Preferences.Set(key, null, sharedName);

			Assert.Null(Preferences.Get(key, null, sharedName));
		}

		[Theory]
		[InlineData("int1", int.MaxValue - 1, null)]
		[InlineData("sint1", int.MinValue + 1, null)]
		[InlineData("int1", int.MaxValue - 1, sharedNameTestData)]
		[InlineData("sint1", int.MinValue + 1, sharedNameTestData)]
		public void Set_Get_Int(string key, int value, string sharedName)
		{
			Preferences.Set(key, value, sharedName);
			Assert.Equal(value, Preferences.Get(key, 0, sharedName));
		}

		[Theory]
		[InlineData("long1", long.MaxValue - 1, null)]
		[InlineData("slong1", long.MinValue + 1, null)]
		[InlineData("long1", long.MaxValue - 1, sharedNameTestData)]
		[InlineData("slong1", long.MinValue + 1, sharedNameTestData)]
		public void Set_Get_Long(string key, long value, string sharedName)
		{
			Preferences.Set(key, value, sharedName);
			Assert.Equal(value, Preferences.Get(key, 0L, sharedName));
		}

		[Theory]
		[InlineData("float1", float.MaxValue - 1, null)]
		[InlineData("sfloat1", float.MinValue + 1, null)]
		[InlineData("float1", float.MaxValue - 1, sharedNameTestData)]
		[InlineData("sfloat1", float.MinValue + 1, sharedNameTestData)]
		public void Set_Get_Float(string key, float value, string sharedName)
		{
			Preferences.Set(key, value, sharedName);
			Assert.Equal(value, Preferences.Get(key, 0f, sharedName));
		}

		[Theory]
		[InlineData("double1", double.MaxValue - 1, null)]
		[InlineData("sdouble1", double.MinValue + 1, null)]
		[InlineData("double1", double.MaxValue - 1, sharedNameTestData)]
		[InlineData("sdouble1", double.MinValue + 1, sharedNameTestData)]
		public void Set_Get_Double(string key, double value, string sharedName)
		{
			Preferences.Set(key, value, sharedName);
			Assert.Equal(value, Preferences.Get(key, 0d, sharedName));
		}

		[Theory]
		[InlineData("bool1", true, null)]
		[InlineData("bool1", true, sharedNameTestData)]
		public void Set_Get_Bool(string key, bool value, string sharedName)
		{
			Preferences.Set(key, value, sharedName);
			Assert.Equal(value, Preferences.Get(key, false, sharedName));
		}

		[Theory]
		[InlineData(null)]
		[InlineData(sharedNameTestData)]
		public void Remove(string sharedName)
		{
			Preferences.Set("RemoveKey1", "value", sharedName);

			Assert.Equal("value", Preferences.Get("RemoveKey1", null, sharedName));

			Preferences.Remove("RemoveKey1", sharedName);

			Assert.Null(Preferences.Get("RemoveKey1", null, sharedName));
		}

		[Theory]
		[InlineData(null, true)]
		[InlineData(sharedNameTestData, true)]
		[InlineData(null, false)]
		[InlineData(sharedNameTestData, false)]
		public void Remove_Get_Bool(string sharedName, bool defaultValue)
		{
			Preferences.Remove("RemoveGetKey1", sharedName);

			Assert.Equal(defaultValue, Preferences.Get("RemoveGetKey1", defaultValue, sharedName));
		}

		[Theory]
		[InlineData(null, 5)]
		[InlineData(sharedNameTestData, 5)]
		[InlineData(null, 0)]
		[InlineData(sharedNameTestData, 0)]
		public void Remove_Get_Double(string sharedName, double defaultValue)
		{
			Preferences.Remove("RemoveGetKey1", sharedName);

			Assert.Equal(defaultValue, Preferences.Get("RemoveGetKey1", defaultValue, sharedName));
		}

		[Theory]
		[InlineData(null, 5)]
		[InlineData(sharedNameTestData, 5)]
		[InlineData(null, 0)]
		[InlineData(sharedNameTestData, 0)]
		public void Remove_Get_Float(string sharedName, float defaultValue)
		{
			Preferences.Remove("RemoveGetKey1", sharedName);

			Assert.Equal(defaultValue, Preferences.Get("RemoveGetKey1", defaultValue, sharedName));
		}

		[Theory]
		[InlineData(null, 5)]
		[InlineData(sharedNameTestData, 5)]
		[InlineData(null, 0)]
		[InlineData(sharedNameTestData, 0)]
		public void Remove_Get_Int(string sharedName, int defaultValue)
		{
			Preferences.Remove("RemoveGetKey1", sharedName);

			Assert.Equal(defaultValue, Preferences.Get("RemoveGetKey1", defaultValue, sharedName));
		}

		[Theory]
		[InlineData(null, 5)]
		[InlineData(sharedNameTestData, 5)]
		[InlineData(null, 0)]
		[InlineData(sharedNameTestData, 0)]
		public void Remove_Get_Long(string sharedName, long defaultValue)
		{
			Preferences.Remove("RemoveGetKey1", sharedName);

			Assert.Equal(defaultValue, Preferences.Get("RemoveGetKey1", defaultValue, sharedName));
		}

		[Theory]
		[InlineData(null, "text")]
		[InlineData(sharedNameTestData, "text")]
		[InlineData(null, null)]
		[InlineData(sharedNameTestData, null)]
		public void Remove_Get_String(string sharedName, string defaultValue)
		{
			Preferences.Remove("RemoveGetKey1", sharedName);

			Assert.Equal(defaultValue, Preferences.Get("RemoveGetKey1", defaultValue, sharedName));
		}

		[Theory]
		[InlineData(null)]
		[InlineData(sharedNameTestData)]
		public void Clear(string sharedName)
		{
			Preferences.Set("ClearKey1", "value", sharedName);
			Preferences.Set("ClearKey2", 2, sharedName);

			Assert.Equal(2, Preferences.Get("ClearKey2", 0, sharedName));

			Preferences.Clear(sharedName);

			Assert.NotEqual("value", Preferences.Get("ClearKey1", null, sharedName));
			Assert.NotEqual(2, Preferences.Get("ClearKey2", 0, sharedName));
		}

		[Theory]
		[InlineData(null)]
		[InlineData(sharedNameTestData)]
		public void Does_ContainsKey(string sharedName)
		{
			Preferences.Set("DoesContainsKey1", "One", sharedName);

			Assert.True(Preferences.ContainsKey("DoesContainsKey1", sharedName));
		}

		[Theory]
		[InlineData(null)]
		[InlineData(sharedNameTestData)]
		public void Not_ContainsKey(string sharedName)
		{
			Preferences.Remove("NotContainsKey1", sharedName);

			Assert.False(Preferences.ContainsKey("NotContainsKey1", sharedName));
		}

		[Theory]
		[InlineData(null, DateTimeKind.Utc)]
		[InlineData(sharedNameTestData, DateTimeKind.Utc)]
		[InlineData(null, DateTimeKind.Local)]
		[InlineData(sharedNameTestData, DateTimeKind.Local)]
		public void DateTimePreservesKind(string sharedName, DateTimeKind kind)
		{
			var date = new DateTime(2018, 05, 07, 8, 30, 0, kind);

			Preferences.Set("datetime_utc", date, sharedName);

			var get = Preferences.Get("datetime_utc", DateTime.MinValue, sharedName);

			Assert.Equal(date, get);
			Assert.Equal(kind, get.Kind);
		}

		#endregion

		#region Non-static method tests

		[Fact]
		public void FailsWithUnsupportedType() =>
			Assert.Throws<NotSupportedException>(() => Preferences.Default.Set("anything", new int[] { 1 }));

		[Theory]
		[InlineData("datetime1", null)]
		[InlineData("datetime1", sharedNameTestData)]
		public void Set_Get_DateTime_NonStatic(string key, string sharedName)
		{
			Preferences.Default.Set(key, testDateTime, sharedName);

			Assert.Equal(testDateTime, Preferences.Default.Get(key, DateTime.MinValue, sharedName));
		}

		[Theory]
		[InlineData("string1", "TEST", null)]
		[InlineData("string1", "TEST", sharedNameTestData)]
		public void Set_Get_String_NonStatic(string key, string value, string sharedName)
		{
			Preferences.Default.Set(key, value, sharedName);

			Assert.Equal(value, Preferences.Default.Get<string>(key, null, sharedName));
		}

		[Theory]
		[InlineData("string1", "TEST", null)]
		[InlineData("string1", "TEST", sharedNameTestData)]
		public void Set_Set_Null_Get_String_NonStatic(string key, string value, string sharedName)
		{
			Preferences.Default.Set(key, value, sharedName);
			Preferences.Default.Set<string>(key, null, sharedName);

			Assert.Null(Preferences.Default.Get<string>(key, null, sharedName));
		}

		[Theory]
		[InlineData("int1", int.MaxValue - 1, null)]
		[InlineData("sint1", int.MinValue + 1, null)]
		[InlineData("int1", int.MaxValue - 1, sharedNameTestData)]
		[InlineData("sint1", int.MinValue + 1, sharedNameTestData)]
		public void Set_Get_Int_NonStatic(string key, int value, string sharedName)
		{
			Preferences.Default.Set(key, value, sharedName);
			Assert.Equal(value, Preferences.Default.Get(key, 0, sharedName));
		}

		[Theory]
		[InlineData("long1", long.MaxValue - 1, null)]
		[InlineData("slong1", long.MinValue + 1, null)]
		[InlineData("long1", long.MaxValue - 1, sharedNameTestData)]
		[InlineData("slong1", long.MinValue + 1, sharedNameTestData)]
		public void Set_Get_Long_NonStatic(string key, long value, string sharedName)
		{
			Preferences.Default.Set(key, value, sharedName);
			Assert.Equal(value, Preferences.Default.Get(key, 0L, sharedName));
		}

		[Theory]
		[InlineData("float1", float.MaxValue - 1, null)]
		[InlineData("sfloat1", float.MinValue + 1, null)]
		[InlineData("float1", float.MaxValue - 1, sharedNameTestData)]
		[InlineData("sfloat1", float.MinValue + 1, sharedNameTestData)]
		public void Set_Get_Float_NonStatic(string key, float value, string sharedName)
		{
			Preferences.Default.Set(key, value, sharedName);
			Assert.Equal(value, Preferences.Default.Get(key, 0f, sharedName));
		}

		[Theory]
		[InlineData("double1", double.MaxValue - 1, null)]
		[InlineData("sdouble1", double.MinValue + 1, null)]
		[InlineData("double1", double.MaxValue - 1, sharedNameTestData)]
		[InlineData("sdouble1", double.MinValue + 1, sharedNameTestData)]
		public void Set_Get_Double_NonStatic(string key, double value, string sharedName)
		{
			Preferences.Default.Set(key, value, sharedName);
			Assert.Equal(value, Preferences.Default.Get(key, 0d, sharedName));
		}

		[Theory]
		[InlineData("bool1", true, null)]
		[InlineData("bool1", true, sharedNameTestData)]
		public void Set_Get_Bool_NonStatic(string key, bool value, string sharedName)
		{
			Preferences.Default.Set(key, value, sharedName);
			Assert.Equal(value, Preferences.Default.Get(key, false, sharedName));
		}

		[Theory]
		[InlineData(null)]
		[InlineData(sharedNameTestData)]
		public void Remove_NonStatic(string sharedName)
		{
			Preferences.Default.Set("RemoveKey1", "value", sharedName);

			Assert.Equal("value", Preferences.Default.Get<string>("RemoveKey1", null, sharedName));

			Preferences.Default.Remove("RemoveKey1", sharedName);

			Assert.Null(Preferences.Default.Get<string>("RemoveKey1", null, sharedName));
		}

		[Theory]
		[InlineData(null, true)]
		[InlineData(sharedNameTestData, true)]
		[InlineData(null, false)]
		[InlineData(sharedNameTestData, false)]
		public void Remove_Get_Bool_NonStatic(string sharedName, bool defaultValue)
		{
			Preferences.Default.Remove("RemoveGetKey1", sharedName);

			Assert.Equal(defaultValue, Preferences.Default.Get("RemoveGetKey1", defaultValue, sharedName));
		}

		[Theory]
		[InlineData(null, 5)]
		[InlineData(sharedNameTestData, 5)]
		[InlineData(null, 0)]
		[InlineData(sharedNameTestData, 0)]
		public void Remove_Get_Double_NonStatic(string sharedName, double defaultValue)
		{
			Preferences.Default.Remove("RemoveGetKey1", sharedName);

			Assert.Equal(defaultValue, Preferences.Default.Get("RemoveGetKey1", defaultValue, sharedName));
		}

		[Theory]
		[InlineData(null, 5)]
		[InlineData(sharedNameTestData, 5)]
		[InlineData(null, 0)]
		[InlineData(sharedNameTestData, 0)]
		public void Remove_Get_Float_NonStatic(string sharedName, float defaultValue)
		{
			Preferences.Default.Remove("RemoveGetKey1", sharedName);

			Assert.Equal(defaultValue, Preferences.Default.Get("RemoveGetKey1", defaultValue, sharedName));
		}

		[Theory]
		[InlineData(null, 5)]
		[InlineData(sharedNameTestData, 5)]
		[InlineData(null, 0)]
		[InlineData(sharedNameTestData, 0)]
		public void Remove_Get_Int_NonStatic(string sharedName, int defaultValue)
		{
			Preferences.Default.Remove("RemoveGetKey1", sharedName);

			Assert.Equal(defaultValue, Preferences.Default.Get("RemoveGetKey1", defaultValue, sharedName));
		}

		[Theory]
		[InlineData(null, 5)]
		[InlineData(sharedNameTestData, 5)]
		[InlineData(null, 0)]
		[InlineData(sharedNameTestData, 0)]
		public void Remove_Get_Long_NonStatic(string sharedName, long defaultValue)
		{
			Preferences.Default.Remove("RemoveGetKey1", sharedName);

			Assert.Equal(defaultValue, Preferences.Default.Get("RemoveGetKey1", defaultValue, sharedName));
		}

		[Theory]
		[InlineData(null, "text")]
		[InlineData(sharedNameTestData, "text")]
		[InlineData(null, null)]
		[InlineData(sharedNameTestData, null)]
		public void Remove_Get_String_NonStatic(string sharedName, string defaultValue)
		{
			Preferences.Default.Remove("RemoveGetKey1", sharedName);

			Assert.Equal(defaultValue, Preferences.Default.Get("RemoveGetKey1", defaultValue, sharedName));
		}

		[Theory]
		[InlineData(null)]
		[InlineData(sharedNameTestData)]
		public void Clear_NonStatic(string sharedName)
		{
			Preferences.Default.Set("ClearKey1", "value", sharedName);
			Preferences.Default.Set("ClearKey2", 2, sharedName);

			Assert.Equal(2, Preferences.Default.Get("ClearKey2", 0, sharedName));

			Preferences.Default.Clear(sharedName);

			Assert.NotEqual("value", Preferences.Default.Get<string>("ClearKey1", null, sharedName));
			Assert.NotEqual(2, Preferences.Default.Get("ClearKey2", 0, sharedName));
		}

		[Theory]
		[InlineData(null)]
		[InlineData(sharedNameTestData)]
		public void Does_ContainsKey_NonStatic(string sharedName)
		{
			Preferences.Default.Set("DoesContainsKey1", "One", sharedName);

			Assert.True(Preferences.Default.ContainsKey("DoesContainsKey1", sharedName));
		}

		[Theory]
		[InlineData(null)]
		[InlineData(sharedNameTestData)]
		public void Not_ContainsKey_NonStatic(string sharedName)
		{
			Preferences.Default.Remove("NotContainsKey1", sharedName);

			Assert.False(Preferences.Default.ContainsKey("NotContainsKey1", sharedName));
		}

		[Theory]
		[InlineData(null, DateTimeKind.Utc)]
		[InlineData(sharedNameTestData, DateTimeKind.Utc)]
		[InlineData(null, DateTimeKind.Local)]
		[InlineData(sharedNameTestData, DateTimeKind.Local)]
		public void DateTimePreservesKind_NonStatic(string sharedName, DateTimeKind kind)
		{
			var date = new DateTime(2018, 05, 07, 8, 30, 0, kind);

			Preferences.Default.Set("datetime_utc", date, sharedName);

			var get = Preferences.Default.Get("datetime_utc", DateTime.MinValue, sharedName);

			Assert.Equal(date, get);
			Assert.Equal(kind, get.Kind);
		}
		#endregion
	}
}
