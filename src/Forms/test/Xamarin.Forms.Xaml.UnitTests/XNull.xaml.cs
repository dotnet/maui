using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class XNull : ContentPage
	{
		public XNull()
		{
			InitializeComponent();
		}

		public XNull(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void SupportsXNull(bool useCompiledXaml)
			{
				var layout = new XNull(useCompiledXaml);
				Assert.True(layout.Resources.ContainsKey("null"));
				Assert.Null(layout.Resources["null"]);
			}
		}
	}
}