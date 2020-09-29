using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ImageButtonTests
		: CommandSourceTests<ImageButton>
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices(getStreamAsync: GetStreamAsync);
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void TestSizing()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			var result = image.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);

			Assert.AreEqual(100, result.Request.Width);
			Assert.AreEqual(20, result.Request.Height);
		}

		[Test]
		public void TestAspectSizingWithConstrainedHeight()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			var result = image.GetSizeRequest(double.PositiveInfinity, 10);

			Assert.AreEqual(50, result.Request.Width);
			Assert.AreEqual(10, result.Request.Height);
		}

		[Test]
		public void TestAspectSizingWithConstrainedWidth()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			var result = image.GetSizeRequest(25, double.PositiveInfinity);

			Assert.AreEqual(25, result.Request.Width);
			Assert.AreEqual(5, result.Request.Height);
		}


		[Test]
		public void TestAspectFillSizingWithConstrainedHeight()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			image.Aspect = Aspect.AspectFill;
			var result = image.GetSizeRequest(double.PositiveInfinity, 10);

			Assert.AreEqual(50, result.Request.Width);
			Assert.AreEqual(10, result.Request.Height);
		}

		[Test]
		public void TestAspectFillSizingWithConstrainedWidth()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			image.Aspect = Aspect.AspectFill;
			var result = image.GetSizeRequest(25, double.PositiveInfinity);

			Assert.AreEqual(25, result.Request.Width);
			Assert.AreEqual(5, result.Request.Height);
		}

		[Test]
		public void TestFillSizingWithConstrainedHeight()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			image.Aspect = Aspect.AspectFill;
			var result = image.GetSizeRequest(double.PositiveInfinity, 10);

			Assert.AreEqual(50, result.Request.Width);
			Assert.AreEqual(10, result.Request.Height);
		}

		[Test]
		public void TestFillSizingWithConstrainedWidth()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			image.Aspect = Aspect.AspectFill;
			var result = image.GetSizeRequest(25, double.PositiveInfinity);

			Assert.AreEqual(25, result.Request.Width);
			Assert.AreEqual(5, result.Request.Height);
		}

		[Test]
		public void TestSizeChanged()
		{
			var image = new ImageButton { Source = "File0.png" };
			Assert.AreEqual("File0.png", ((FileImageSource)image.Source).File);

			var preferredSizeChanged = false;
			image.MeasureInvalidated += (sender, args) => preferredSizeChanged = true;

			image.Source = "File1.png";
			Assert.AreEqual("File1.png", ((FileImageSource)image.Source).File);
			Assert.True(preferredSizeChanged);
		}

		[Test]
		public void TestSource()
		{
			var image = new ImageButton();

			Assert.IsNull(image.Source);

			bool signaled = false;
			image.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == "Source")
					signaled = true;
			};

			var source = ImageSource.FromFile("File.png");
			image.Source = source;

			Assert.AreEqual(source, image.Source);
			Assert.True(signaled);
		}

		[Test]
		public void TestSourceDoubleSet()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png") };

			bool signaled = false;
			image.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == "Source")
					signaled = true;
			};

			image.Source = image.Source;

			Assert.False(signaled);
		}

		[Test]
		public void TestFileImageSourceChanged()
		{
			var source = (FileImageSource)ImageSource.FromFile("File.png");

			bool signaled = false;
			source.SourceChanged += (sender, e) =>
			{
				signaled = true;
			};

			source.File = "Other.png";
			Assert.AreEqual("Other.png", source.File);

			Assert.True(signaled);
		}

		[Test]
		public void TestFileImageSourcePropertiesChangedTriggerResize()
		{
			var source = new FileImageSource();
			var image = new ImageButton { Source = source };
			bool fired = false;
			image.MeasureInvalidated += (sender, e) => fired = true;
			Assert.Null(source.File);
			source.File = "foo.png";
			Assert.NotNull(source.File);
			Assert.True(fired);
		}

		[Test]
		public void TestStreamImageSourcePropertiesChangedTriggerResize()
		{
			var source = new StreamImageSource();
			var image = new ImageButton { Source = source };
			bool fired = false;
			EventHandler eventHandler = (sender, e) => fired = true;
			image.MeasureInvalidated += eventHandler;
			Assert.Null(source.Stream);
			source.Stream = token => Task.FromResult<Stream>(null);
			Assert.NotNull(source.Stream);
			Assert.True(fired);
		}

		[Test]
		public void TestImageSourceToNullCancelsLoading()
		{
			var image = new ImageButton();
			var mockImageRenderer = new MockImageRenderer(image);
			var loader = new UriImageSource { Uri = new Uri("http://www.public-domain-image.com/free-images/miscellaneous/big-high-border-fence.jpg") };
			image.Source = loader;
			Assert.IsTrue(image.IsLoading);
			image.Source = null;
			Assert.IsFalse(image.IsLoading);
			Assert.IsTrue(cancelled);
		}

		static bool cancelled;

		static async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			try
			{
				await Task.Delay(5000, cancellationToken);
			}
			catch (TaskCanceledException ex)
			{
				cancelled = true;
				throw ex;
			}

			if (cancellationToken.IsCancellationRequested)
			{
				cancelled = true;
				throw new TaskCanceledException();
			}

			var stream = typeof(ImageTests).Assembly.GetManifestResourceStream(uri.LocalPath.Substring(1));
			return stream;
		}

		class MockImageRenderer
		{
			public MockImageRenderer(ImageButton element)
			{
				Element = element;
				Element.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName == nameof(ImageButton.Source))
						Load();
				};
			}

			public ImageButton Element { get; set; }

			public async void Load()
			{
				if (initialLoad && Element.Source != null)
				{
					initialLoad = false;
					Element.SetIsLoading(true);
					await (Element.Source as UriImageSource).GetStreamAsync();
					Element.SetIsLoading(false);
				}
			}

			bool initialLoad = true;
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void TestClickedvent(bool isEnabled)
		{
			var view = new ImageButton()
			{
				IsEnabled = isEnabled,
			};

			bool activated = false;
			view.Clicked += (sender, e) => activated = true;

			((IButtonController)view).SendClicked();

			Assert.True(activated == isEnabled ? true : false);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void TestPressedEvent(bool isEnabled)
		{
			var view = new ImageButton()
			{
				IsEnabled = isEnabled,
			};

			bool pressed = false;
			view.Pressed += (sender, e) => pressed = true;

			((IButtonController)view).SendPressed();

			Assert.True(pressed == isEnabled ? true : false);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void TestReleasedEvent(bool isEnabled)
		{
			var view = new ImageButton()
			{
				IsEnabled = isEnabled,
			};

			bool released = false;
			view.Released += (sender, e) => released = true;

			((IButtonController)view).SendReleased();

			Assert.True(released == isEnabled ? true : false);
		}

		protected override ImageButton CreateSource()
		{
			return new ImageButton();
		}

		protected override void Activate(ImageButton source)
		{
			((IButtonController)source).SendClicked();
		}

		protected override BindableProperty IsEnabledProperty
		{
			get { return ImageButton.IsEnabledProperty; }
		}

		protected override BindableProperty CommandProperty
		{
			get { return ImageButton.CommandProperty; }
		}

		protected override BindableProperty CommandParameterProperty
		{
			get { return ImageButton.CommandParameterProperty; }
		}


		[Test]
		public void TestBindingContextPropagation()
		{
			var context = new object();
			var button = new ImageButton();
			button.BindingContext = context;
			var source = new FileImageSource();
			button.Source = source;
			Assert.AreSame(context, source.BindingContext);

			button = new ImageButton();
			source = new FileImageSource();
			button.Source = source;
			button.BindingContext = context;
			Assert.AreSame(context, source.BindingContext);
		}

		[Test]
		public void TestImageSourcePropertiesChangedTriggerResize()
		{
			var source = new FileImageSource();
			var button = new ImageButton { Source = source };
			bool fired = false;
			button.MeasureInvalidated += (sender, e) => fired = true;
			Assert.Null(source.File);
			source.File = "foo.png";
			Assert.NotNull(source.File);
			Assert.True(fired);
		}


		[Test]
		public void CommandCanExecuteUpdatesEnabled()
		{
			var button = new ImageButton();

			bool result = false;

			var bindingContext = new
			{
				Command = new Command(() => { }, () => result)
			};

			button.SetBinding(ImageButton.CommandProperty, "Command");
			button.BindingContext = bindingContext;

			Assert.False(button.IsEnabled);

			result = true;

			bindingContext.Command.ChangeCanExecute();

			Assert.True(button.IsEnabled);
		}

		[Test]
		public void ButtonClickWhenCommandCanExecuteFalse()
		{
			bool invoked = false;
			var button = new ImageButton()
			{
				Command = new Command(() => invoked = true
				, () => false),
			};

			(button as IButtonController)
				?.SendClicked();

			Assert.False(invoked);
		}
	}
}