using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
#if DEBUG
	public partial class Maui2418 : ContentPage
	{
		public Maui2418() => InitializeComponent();
		public Maui2418(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void Method(bool useCompiledXaml)
			{
				var page = new Maui2418(useCompiledXaml);
				Assert.NotNull(page);
				var label0 = page.label0;
				var sourceInfo = VisualDiagnostics.GetSourceInfo(label0);
				Assert.Equal($"Issues{System.IO.Path.DirectorySeparatorChar}Maui2418.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", sourceInfo.SourceUri.OriginalString);
			}
		}
	}
#endif
}
