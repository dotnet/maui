using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Gh4238
	{
		public System.Collections.ArrayList Property { get; set; }

		[Test]
		public void Test()
		{
			InitializeComponent();
			Assert.AreEqual(0f, Property[0]);
		}
	}
}
