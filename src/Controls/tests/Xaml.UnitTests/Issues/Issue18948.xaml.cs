using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue18948 : Shell
	{
		public Issue18948()
		{
			InitializeComponent();
		}
		public Issue18948(bool useCompiledXaml)
		{
			// This stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void NavBarIsVisiblePropertyPropagates(bool useCompiledXaml)
			{
				var shell = new Issue18948(useCompiledXaml);
				var navBarIsVisible = Shell.GetNavBarIsVisible(shell.CurrentContent);
				Assert.False(navBarIsVisible);
			}
		}
	}
}

