using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class SharedResourceDictionary : ResourceDictionary
{
	public SharedResourceDictionary() => InitializeComponent();

	[TestFixture]
	public class Tests
	{
		[Test]
		public void ResourcesDirectoriesCanBeXamlRoots([Values] XamlInflator inflator)
		{
			var layout = new SharedResourceDictionary(inflator);
			Assert.AreEqual(5, layout.Count);
		}
	}
}