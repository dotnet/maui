using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Gh4238
	{
		public System.Collections.ArrayList Property { get; set; }

		[Fact]
		public void Test()
		{
			InitializeComponent();
			Assert.Equal(0f, Property[0]);
		}
	}
}
