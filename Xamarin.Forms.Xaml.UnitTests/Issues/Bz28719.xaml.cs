using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
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
		}

		[TestFixture]
		class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(true)]
			[TestCase(false)]
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
				Assert.AreEqual("Remove.png", (image0.Source as FileImageSource)?.File);

				cell0.BindingContext = new { IsSelected = false };
				Assert.AreEqual("Add.png", (image0.Source as FileImageSource)?.File);

				var cell1 = template.CreateContent() as ViewCell;
				Assert.NotNull(cell1);
				var image1 = cell1.View as Image;
				Assert.NotNull(image1);

				cell1.BindingContext = new { IsSelected = true };
				Assert.AreEqual("Remove.png", (image1.Source as FileImageSource)?.File);

				cell1.BindingContext = new { IsSelected = false };
				Assert.AreEqual("Add.png", (image1.Source as FileImageSource)?.File);
			}
		}
	}
}