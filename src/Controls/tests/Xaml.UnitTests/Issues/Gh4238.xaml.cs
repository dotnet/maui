using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[Collection("Issue")]
	public partial class Gh4238
	{
		public Gh4238() => InitializeComponent();
		
		public System.Collections.ArrayList Property { get; set; }

		[Fact]
		public void Test()
		{
			InitializeComponent();
			Assert.Equal(0f, Property[0]);
		}
	}
}
