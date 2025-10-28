using Microsoft.Maui.Controls;
using Xunit;

namespace Foo.Microsoft.Maui.Controls.Bar
{
	public partial class Bz43301 : ContentPage
	{
		public Bz43301()
		{
			InitializeComponent();
		}

		public Bz43301(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true)]
			[InlineData(false)]
			//No need for any actual [Fact]. If this compiles, the bug is fixed.
			public void DoesCompile(bool useCompiledXaml)
			{
				var layout = new Bz43301(useCompiledXaml);
				Assert.Pass();
			}
		}
	}
}