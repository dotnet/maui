using System.Collections;
using System.Linq;
using NUnit.Framework;


namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TitleViewUnitTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void BindingContextPropagatesFromParent()
		{
			NavigationPage navigationPage = new NavigationPage();
			var image1 = new Image();
			image1.SetBinding(Image.SourceProperty, "ImageSource");
			var page = new ContentPage()
			{
				Content = new Label()
			};

			var title = new Label() { Text = "Failed" };
			title.SetBinding(Label.TextProperty, "Title");

			var layout = new StackLayout()
			{
				Orientation = StackOrientation.Horizontal,
				Children =
					{
						title,
						image1
					}
			};

			page.SetValue(NavigationPage.TitleViewProperty, layout);
			navigationPage.PushAsync(page);

			var model = new Model();
			navigationPage.BindingContext = new Model();
			Assert.AreEqual(model.Title, title.Text);

			string success = "Success";
			page.BindingContext = new Model() { Title = success };
			Assert.AreEqual(success, title.Text);
			navigationPage.BindingContext = new Model() { Title = "Failed" };
			Assert.AreEqual(success, title.Text);
		}

		public class Model
		{
			public string Title { get; set; } = "Binding Working";
			public string ImageSource { get; } = "coffee.png";
		}
	}
}