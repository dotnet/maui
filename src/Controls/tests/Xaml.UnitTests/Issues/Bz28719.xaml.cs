using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz28719 : ContentPage
	{
		public Bz28719()
		{
			InitializeComponent();
		}

		public Bz28719(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[InlineData(true)]
			[InlineData(false)]
			public void DataTriggerInTemplates(bool useCompiledXaml)
			{
				var layout = new Bz28719(useCompiledXaml);
				var template = layout.listView.ItemTemplate;
				Assert.NotNull(template);
				var cell0 = template.CreateContent() as ViewCell;
				Assert.NotNull(cell0);
				var image0 = cell0.View as Image;
				Assert.NotNull(image0);

				cell0.BindingContext = new { IsSelected = true };
				Assert.Equal("Remove.png", (image0.Source as FileImageSource)?.File);

				cell0.BindingContext = new { IsSelected = false };
				Assert.Equal("Add.png", (image0.Source as FileImageSource)?.File);

				var cell1 = template.CreateContent() as ViewCell;
				Assert.NotNull(cell1);
				var image1 = cell1.View as Image;
				Assert.NotNull(image1);

				cell1.BindingContext = new { IsSelected = true };
				Assert.Equal("Remove.png", (image1.Source as FileImageSource)?.File);

				cell1.BindingContext = new { IsSelected = false };
				Assert.Equal("Add.png", (image1.Source as FileImageSource)?.File);
			}
		}
	}
}