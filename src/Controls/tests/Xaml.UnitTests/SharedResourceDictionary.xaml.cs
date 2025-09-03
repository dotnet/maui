using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class SharedResourceDictionary : ResourceDictionary
{
	public SharedResourceDictionary() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ResourcesDirectoriesCanBeXamlRoots([Values] XamlInflator inflator)
		{
			var layout = new SharedResourceDictionary(inflator);
			Assert.AreEqual(5, layout.Count);
		}
	}
}