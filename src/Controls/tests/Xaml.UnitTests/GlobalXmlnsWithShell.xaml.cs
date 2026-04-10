using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GlobalXmlnsWithShell : Shell
{
	public GlobalXmlnsWithShell() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ShellWithoutXDeclaration(XamlInflator inflator)
		{
			// Verifies that a Shell XAML file (like AppShell.xaml in the default template) that uses
			// x:Class and x:Name without an explicit xmlns:x declaration can be inflated.
			// Regression test for https://github.com/dotnet/maui/issues/28836 and the net11.0 template build failure.
			var shell = new GlobalXmlnsWithShell(inflator);
			Assert.NotNull(shell);
			Assert.NotNull(shell.homeContent);
		}
	}
}
