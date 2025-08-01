using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz53318ListView : ListView
{
	public Bz53318ListView([Parameter("CachingStrategy")] ListViewCachingStrategy cachingStrategy) : base(cachingStrategy) { }
}

public partial class Bz53318 : ContentPage
{
	public Bz53318() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void DoesCompilesArgsInsideDataTemplate([Values(XamlInflator.XamlC)] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Bz53318)));
		}
	}
}