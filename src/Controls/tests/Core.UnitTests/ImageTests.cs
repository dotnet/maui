using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ImageTests : BaseTestFixture
	{
		[Fact]
		public void TestSizing()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			var result = image.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

			Assert.Equal(100, result.Request.Width);
			Assert.Equal(20, result.Request.Height);
		}

		[Fact]
		public void TestAspectSizingWithConstrainedHeight()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			var result = image.Measure(double.PositiveInfinity, 10, MeasureFlags.None);

			Assert.Equal(50, result.Request.Width);
			Assert.Equal(10, result.Request.Height);
		}

		[Fact]
		public void TestAspectSizingWithConstrainedWidth()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			var result = image.Measure(25, double.PositiveInfinity, MeasureFlags.None);

			Assert.Equal(25, result.Request.Width);
			Assert.Equal(5, result.Request.Height);
		}

		[Fact]
		public void TestAspectFillSizingWithConstrainedHeight()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			image.Aspect = Aspect.AspectFill;
			var result = image.Measure(double.PositiveInfinity, 10, MeasureFlags.None);

			Assert.Equal(50, result.Request.Width);
			Assert.Equal(10, result.Request.Height);
		}

		[Fact]
		public void TestAspectFillSizingWithConstrainedWidth()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			image.Aspect = Aspect.AspectFill;
			var result = image.Measure(25, double.PositiveInfinity, MeasureFlags.None);

			Assert.Equal(25, result.Request.Width);
			Assert.Equal(5, result.Request.Height);
		}

		[Fact]
		public void TestFillSizingWithConstrainedHeight()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			image.Aspect = Aspect.AspectFill;
			var result = image.Measure(double.PositiveInfinity, 10, MeasureFlags.None);

			Assert.Equal(50, result.Request.Width);
			Assert.Equal(10, result.Request.Height);
		}

		[Fact]
		public void TestFillSizingWithConstrainedWidth()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), Handler = new SizedHandler() };

			image.Aspect = Aspect.AspectFill;
			var result = image.Measure(25, double.PositiveInfinity, MeasureFlags.None);

			Assert.Equal(25, result.Request.Width);
			Assert.Equal(5, result.Request.Height);
		}

		[Fact]
		public void TestSizeChanged()
		{
			var image = new Image { Source = "File0.png" };
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
			var image = new Image();

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
			var image = new Image { Source = ImageSource.FromFile("File.png") };

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
			var image = new Image { Source = source };
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
			var image = new Image { Source = source };
			bool fired = false;
			image.MeasureInvalidated += (sender, e) => fired = true;
			Assert.Null(source.Stream);
			source.Stream = token => Task.FromResult<Stream>(null);
			Assert.NotNull(source.Stream);
			Assert.True(fired);
		}

		[Fact]
		public void TestImageSourceToNullCancelsLoading()
		{
			var cancelled = false;

			var image = new Image();
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
				catch (TaskCanceledException)
				{
					cancelled = true;
					throw;
				}

				if (cancellationToken.IsCancellationRequested)
				{
					cancelled = true;
					throw new TaskCanceledException();
				}

				return typeof(ImageTests).Assembly.GetManifestResourceStream("dummy");
			}
		}

		class MockImageRenderer
		{
			public MockImageRenderer(Image element)
			{
				Element = element;
				Element.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName == nameof(Image.Source))
						Load();
				};
			}

			public Image Element { get; set; }

			public TaskCompletionSource<bool> CompletionSource { get; private set; } = new TaskCompletionSource<bool>();

			public async void Load()
			{
				if (initialLoad && Element.Source != null)
				{
					initialLoad = false;
					var controller = (IImageController)Element;
					try
					{
						controller.SetIsLoading(true);
						await ((IStreamImageSource)Element.Source).GetStreamAsync();
					}
					catch (OperationCanceledException)
					{
						// this is expected
					}
					finally
					{
						controller.SetIsLoading(false);
						CompletionSource.SetResult(true);
					}
				}
			}

			bool initialLoad = true;
		}

		class SizedHandler : ImageHandler
		{
			Size _size;

			public SizedHandler(Size size) => _size = size;

			public SizedHandler() => _size = new(100, 20);

			protected override object CreatePlatformView() => new();

			public override Size GetDesiredSize(double widthConstraint, double heightConstraint) => _size;
		}
	}
}
