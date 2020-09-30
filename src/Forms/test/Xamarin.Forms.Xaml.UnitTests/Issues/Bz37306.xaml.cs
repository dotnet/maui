using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Bz37306 : ContentPage
	{
		public Bz37306()
		{
			InitializeComponent();
		}

		public Bz37306(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void xStringInResourcesDictionaries(bool useCompiledXaml)
			{
				var layout = new Bz37306(useCompiledXaml);
				Assert.AreEqual("Mobile App", layout.Resources["AppName"]);
				Assert.AreEqual("Mobile App", layout.Resources["ApplicationName"]);
			}
		}
	}
}