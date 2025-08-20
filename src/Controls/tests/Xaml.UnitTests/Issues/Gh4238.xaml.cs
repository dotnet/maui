using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public partial class Gh4238
	{
		public Gh4238() => InitializeComponent();
		
		public System.Collections.ArrayList Property { get; set; }

		[Test]
		public void Test()
		{
			InitializeComponent();
			Assert.AreEqual(0f, Property[0]);
		}
	}
}
