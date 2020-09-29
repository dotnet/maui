using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Issue2062 : ContentPage
	{
		public Issue2062()
		{
			InitializeComponent();
		}

		public Issue2062(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void LabelWithoutExplicitPropertyElement(bool useCompiledXaml)
			{
				var layout = new Issue2062(useCompiledXaml);
				Assert.AreEqual("text explicitly set to Label.Text", layout.label1.Text);
				Assert.AreEqual("text implicitly set to Text property of Label", layout.label2.Text);
			}
		}
	}
}