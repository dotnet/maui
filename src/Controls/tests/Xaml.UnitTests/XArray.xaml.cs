using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[ContentProperty("Content")]
public class MockBindableForArray : View
{
	public object Content { get; set; }
}

public partial class XArray : MockBindableForArray
{
	public XArray() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void SupportsXArray(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation().WithAdditionalSource(
"""
using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[ContentProperty("Content")]
public class MockBindableForArray : View
{
	public object Content { get; set; }
}

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class XArray : MockBindableForArray
{
	public XArray() => InitializeComponent();
}
""").RunMauiSourceGenerator(typeof(XArray));
				Assert.Empty(result.Diagnostics);
				return;
			}
			var layout = new XArray(inflator);
			var array = layout.Content;
			Assert.NotNull(array);
			Assert.IsType<string[]>(array);
			Assert.Equal(2, ((string[])layout.Content).Length);
			Assert.Equal("Hello", ((string[])layout.Content)[0]);
			Assert.Equal("World", ((string[])layout.Content)[1]);
		}
	}
}