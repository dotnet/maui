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
	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void ArgumentsInsideDataTemplate(XamlInflator inflator)
		{
			var page = new Bz53318(inflator);
			var list = Assert.IsType<ListView>(page.Content);
			var nestedList = Assert.IsType<Bz53318ListView>(list.ItemTemplate.CreateContent());
			Assert.Equal(ListViewCachingStrategy.RecycleElement, nestedList.CachingStrategy);
		}
	}
}