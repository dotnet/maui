using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using static Microsoft.Maui.Controls.Core.UnitTests.VisualStateTestHelpers;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ImageButtonTests : VisualElementCommandSourceTests<ImageButton>
	{
		[Fact]
		public void TestSizing()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			var result = image.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

			Assert.Equal(100, result.Request.Width);
			Assert.Equal(20, result.Request.Height);
		}

		[Fact]
		public void TestAspectSizingWithConstrainedHeight()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			var result = image.Measure(double.PositiveInfinity, 10, MeasureFlags.None);

			Assert.Equal(50, result.Request.Width);
			Assert.Equal(10, result.Request.Height);
		}

		[Fact]
		public void TestAspectSizingWithConstrainedWidth()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			var result = image.Measure(25, double.PositiveInfinity, MeasureFlags.None);

			Assert.Equal(25, result.Request.Width);
			Assert.Equal(5, result.Request.Height);
		}

		[Fact]
		public void TestAspectFillSizingWithConstrainedHeight()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			image.Aspect = Aspect.AspectFill;
			var result = image.Measure(double.PositiveInfinity, 10, MeasureFlags.None);

			Assert.Equal(50, result.Request.Width);
			Assert.Equal(10, result.Request.Height);
		}

		[Fact]
		public void TestAspectFillSizingWithConstrainedWidth()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			image.Aspect = Aspect.AspectFill;
			var result = image.Measure(25, double.PositiveInfinity, MeasureFlags.None);

			Assert.Equal(25, result.Request.Width);
			Assert.Equal(5, result.Request.Height);
		}

		[Fact]
		public void TestFillSizingWithConstrainedHeight()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			image.Aspect = Aspect.AspectFill;
			var result = image.Measure(double.PositiveInfinity, 10, MeasureFlags.None);

			Assert.Equal(50, result.Request.Width);
			Assert.Equal(10, result.Request.Height);
		}

		[Fact]
		public void TestFillSizingWithConstrainedWidth()
		{
			var image = new ImageButton { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			image.Aspect = Aspect.AspectFill;
			var result = image.Measure(25, double.PositiveInfinity, MeasureFlags.None);

			Assert.Equal(25, result.Request.Width);
			Assert.Equal(5, result.Request.Height);
		}

		[Fact]
		public void TestSizeChanged()
		{
			var image = new ImageButton { Source = "File0.png" };
			Assert.Equal("File0.png", ((FileImageSource)image.Source).File);

			var preferredSizeChanged = false;
			image.MeasureInvalidated += (sender, args) => preferredSizeChanged = true;

			image.Source = "File1.png";
			Assert.Equal("File1.png", ((FileImageSource)image.Source).File);
			Assert.True(preferredSizeChanged);
		}

		[Fact]
		public void TestSource()
		{
			var image = new ImageButton();

			Assert.Null(image.Source);

			bool signaled = false;
			image.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == "Source")
					signaled = true;
			};

			var source = ImageSource.FromFile("File.png");
			image.Source = source;

			Assert.Equal(source, image.Source);
			Assert.True(signaled);
		}

		[Fact]
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

		[Fact]
		public void TestFileImageSourceChanged()
		{
			var source = (FileImageSource)ImageSource.FromFile("File.png");

			bool signaled = false;
			source.SourceChanged += (sender, e) =>
			{
				signaled = true;
			};

			source.File = "Other.png";
			Assert.Equal("Other.png", source.File);

			Assert.True(signaled);
		}

		[Fact]
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

		[Fact]
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

		[Fact]
		public void TestImageSourceToNullCancelsLoading()
		{
			var cancelled = false;

			var image = new ImageButton();
			var mockImageRenderer = new MockImageRenderer(image);
			var loader = new StreamImageSource { Stream = GetStreamAsync };

			image.Source = loader;
			Assert.True(image.IsLoading);

			image.Source = null;
			mockImageRenderer.CompletionSource.Task.Wait();
			Assert.False(image.IsLoading);
			Assert.True(cancelled);

			async Task<Stream> GetStreamAsync(CancellationToken cancellationToken)
			{
				try
				{
					await Task.Delay(5000, cancellationToken);
				}
				catch (TaskCanceledException ex)
				{
					cancelled = true;
					throw;
				}

				if (cancellationToken.IsCancellationRequested)
				{
					cancelled = true;
					throw new TaskCanceledException();
				}

				var stream = typeof(ImageTests).Assembly.GetManifestResourceStream("dummy");
				return stream;
			}
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

			public TaskCompletionSource<bool> CompletionSource { get; private set; } = new TaskCompletionSource<bool>();

			public async void Load()
			{
				if (initialLoad && Element.Source != null)
				{
					initialLoad = false;
					try
					{
						Element.SetIsLoading(true);
						await ((IStreamImageSource)Element.Source).GetStreamAsync();
					}
					catch (OperationCanceledException)
					{
						// this is expected
					}
					finally
					{
						Element.SetIsLoading(false);
						CompletionSource.SetResult(true);
					}
				}
			}

			bool initialLoad = true;
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
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

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
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

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void TestReleasedEvent(bool isEnabled)
		{
			var view = new ImageButton()
			{
				IsEnabled = isEnabled,
			};

			bool released = false;
			view.Released += (sender, e) => released = true;

			((IButtonController)view).SendReleased();

			// Released should always fire, even if the button is disabled
			// Otherwise, a press which disables a button will leave it in the
			// Pressed state forever
			Assert.True(released);
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

		[Fact]
		public void TestBindingContextPropagation()
		{
			var context = new object();
			var button = new ImageButton();
			button.BindingContext = context;
			var source = new FileImageSource();
			button.Source = source;
			Assert.Same(context, source.BindingContext);

			button = new ImageButton();
			source = new FileImageSource();
			button.Source = source;
			button.BindingContext = context;
			Assert.Same(context, source.BindingContext);
		}

		[Fact]
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

		[Fact]
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

		[Fact]
		public void PressedVisualState()
		{
			var vsgList = CreateTestStateGroups();
			var stateGroup = vsgList[0];
			var element = new ImageButton();
			VisualStateManager.SetVisualStateGroups(element, vsgList);

			element.SendPressed();
			Assert.Equal(PressedStateName, stateGroup.CurrentState.Name);

			element.SendReleased();
			Assert.NotEqual(PressedStateName, stateGroup.CurrentState.Name);
		}

		class SizedHandler : ImageButtonHandler
		{
			Size _size;

			public SizedHandler(Size size) => _size = size;

			public SizedHandler() => _size = new(100, 20);

			protected override object CreatePlatformView() => new();

			public override Size GetDesiredSize(double widthConstraint, double heightConstraint) => _size;
		}
	}
}
