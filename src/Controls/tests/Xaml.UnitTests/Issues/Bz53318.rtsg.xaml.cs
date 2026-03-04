using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz53318ListView : ListView
{
	public Bz53318ListView([Parameter("CachingStrategy")] ListViewCachingStrategy cachingStrategy) : base(cachingStrategy) { }
}

public partial class Bz53318 : ContentPage
{
	public Bz53318() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[InlineData(XamlInflator.XamlC)]
		internal void DoesCompilesArgsInsideDataTemplate(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				var ex = Record.Exception(() => MockCompiler.Compile(typeof(Bz53318)));
				Assert.Null(ex);
			}
		}
	}
}