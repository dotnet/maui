using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class SharedResourceDictionary : ResourceDictionary
{
	public SharedResourceDictionary() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void ResourcesDirectoriesCanBeXamlRoots(XamlInflator inflator)
		{
			var layout = new SharedResourceDictionary(inflator);
			Assert.Equal(5, layout.Count);
		}
	}
}