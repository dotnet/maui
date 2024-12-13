using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz53318ListView : ListView
{
}

[XamlCompilation(XamlCompilationOptions.Skip)]
[XamlProcessing(XamlInflator.Runtime|XamlInflator.SourceGen, true)]
public partial class Bz53318 : ContentPage
{
	public Bz53318()
	{
		InitializeComponent();
	}

	[TestFixture]
	public class Tests
	{
		[Test]
		public void DoesCompilesArgsInsideDataTemplate()
		{
			Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Bz53318)));
		}
	}
}