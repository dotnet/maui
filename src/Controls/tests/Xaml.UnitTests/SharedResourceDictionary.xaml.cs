using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class SharedResourceDictionary : ResourceDictionary
{
	public SharedResourceDictionary() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ResourcesDirectoriesCanBeXamlRoots(XamlInflator inflator)
		{
			var layout = new SharedResourceDictionary(inflator);
			Assert.Equal(5, layout.Count);
		}
	}
}