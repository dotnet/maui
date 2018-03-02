using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.DeviceTests
{
    public class Preferences_Tests
    {
        const string sharedName = "Shared";

        [Theory]
        [InlineData("string1", "TEST", null)]
        [InlineData("string1", "TEST", sharedName)]
        public void Set_Get_String(string key, string value, string sharedName)
        {
            var p = new Preferences(sharedName);
            p.Set(key, value);

            Assert.Equal(value, p.Get(key, null));
        }

        [Theory]
        [InlineData("int1", int.MaxValue - 1, null)]
        [InlineData("sint1", int.MinValue + 1, null)]
        [InlineData("int1", int.MaxValue - 1, sharedName)]
        [InlineData("sint1", int.MinValue + 1, sharedName)]
        public void Set_Get_Int(string key, int value, string sharedName)
        {
            var p = new Preferences(sharedName);
            p.Set(key, value);
            Assert.Equal(value, p.Get(key, 0));
        }

        [Theory]
        [InlineData("long1", long.MaxValue - 1, null)]
        [InlineData("slong1", long.MinValue + 1, null)]
        [InlineData("long1", long.MaxValue - 1, sharedName)]
        [InlineData("slong1", long.MinValue + 1, sharedName)]
        public void Set_Get_Long(string key, long value, string sharedName)
        {
            var p = new Preferences(sharedName);
            p.Set(key, value);
            Assert.Equal(value, p.Get(key, 0L));
        }

        [Theory]
        [InlineData("float1", float.MaxValue - 1, null)]
        [InlineData("sfloat1", float.MinValue + 1, null)]
        [InlineData("float1", float.MaxValue - 1, sharedName)]
        [InlineData("sfloat1", float.MinValue + 1, sharedName)]
        public void Set_Get_Float(string key, float value, string sharedName)
        {
            var p = new Preferences(sharedName);
            p.Set(key, value);
            Assert.Equal(value, p.Get(key, 0f));
        }

        [Theory]
        [InlineData("double1", double.MaxValue - 1, null)]
        [InlineData("sdouble1", double.MinValue + 1, null)]
        [InlineData("double1", double.MaxValue - 1, sharedName)]
        [InlineData("sdouble1", double.MinValue + 1, sharedName)]
        public void Set_Get_Double(string key, double value, string sharedName)
        {
            var p = new Preferences(sharedName);
            p.Set(key, value);
            Assert.Equal(value, p.Get(key, 0d));
        }

        [Theory]
        [InlineData("bool1", true, null)]
        [InlineData("bool1", true, sharedName)]
        public void Set_Get_Bool(string key, bool value, string sharedName)
        {
            var p = new Preferences(sharedName);
            p.Set(key, value);
            Assert.Equal(value, p.Get(key, false));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(sharedName)]
        public void Remove(string sharedName)
        {
            var p = new Preferences(sharedName);
            p.Set("RemoveKey1", "value");

            Assert.Equal("value", p.Get("RemoveKey1", null));

            p.Remove("RemoveKey1");

            Assert.Null(p.Get("RemoveKey1", null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(sharedName)]
        public void Clear(string sharedName)
        {
            var p = new Preferences(sharedName);
            p.Set("ClearKey1", "value");
            p.Set("ClearKey2", 2);

            Assert.Equal(2, p.Get("ClearKey2", 0));

            p.Clear();

            Assert.NotEqual("value", p.Get("ClearKey1", null));
            Assert.NotEqual(2, p.Get("ClearKey2", 0));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(sharedName)]
        public void Does_ContainsKey(string sharedName)
        {
            var p = new Preferences(sharedName);
            p.Set("DoesContainsKey1", "One");

            Assert.True(p.ContainsKey("DoesContainsKey1"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(sharedName)]
        public void Not_ContainsKey(string sharedName)
        {
            var p = new Preferences(sharedName);
            p.Remove("NotContainsKey1");

            Assert.False(p.ContainsKey("NotContainsKey1"));
        }
    }
}
