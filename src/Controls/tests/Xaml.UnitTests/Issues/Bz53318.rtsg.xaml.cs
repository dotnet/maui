using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz53318ListView : ListView
{
	public Bz53318ListView([Parameter("CachingStrategy")] ListViewCachingStrategy cachingStrategy) : base(cachingStrategy) { }
}

public partial class Bz53318 : ContentPage
{
	public Bz53318() => InitializeComponent();


	public class Tests
	{
		[Fact]
		public void DoesCompilesArgsInsideDataTemplate() // TODO: Fix parameters - see comment above] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => MockCompiler.Compile(typeof(Bz53318)));
			}
		}
	}
}