using Microsoft.Maui.Platform;
using System.Reflection;
using Xunit;

namespace Microsoft.Maui.Graphics.DeviceTests;

public class ColorTests
{
	[Fact]
	public void TestEquals()
	{
		var fields = typeof(Colors).GetFields(BindingFlags.Public | BindingFlags.Static);
		foreach (FieldInfo field in fields)
		{
			Color expected = (Color)field.GetValue(null);

#if WINDOWS
			var actual = (expected.ToPlatform() as UI.Xaml.Media.SolidColorBrush).ToColor();
#else
			var actual = expected.ToPlatform().ToColor();
#endif

			var isEquals = expected.Equals(actual);
			Assert.True(isEquals);
		}
	}
}
