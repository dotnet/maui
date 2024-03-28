using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz19380 : ContentPage
	{
		public Bz19380()
		{
			InitializeComponent();
		}

		public Bz19380(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void ShouldOverrideThumbAndOnColorsFromResources(bool useCompiledXaml)
			{
				var layout = new Bz19380(useCompiledXaml);

				var switch1 = layout.Switch1;

				Assert.True(switch1.OnColor == Colors.Red);
				Assert.True(switch1.ThumbColor == Colors.Blue);

				switch1.IsToggled = true;

				Assert.True(switch1.OnColor == Colors.Red);
				Assert.True(switch1.ThumbColor == Colors.Blue);
			}
		}
	}
}