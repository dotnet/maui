using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ImageTests : BaseTestFixture
	{
		[Test]
		public void TestSizing()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			var result = image.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.AreEqual(100, result.Request.Width);
			Assert.AreEqual(20, result.Request.Height);
		}

		[Test]
		public void TestAspectSizingWithConstrainedHeight()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			var result = image.Measure(double.PositiveInfinity, 10);

			Assert.AreEqual(50, result.Request.Width);
			Assert.AreEqual(10, result.Request.Height);
		}

		[Test]
		public void TestAspectSizingWithConstrainedWidth()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			var result = image.Measure(25, double.PositiveInfinity);

			Assert.AreEqual(25, result.Request.Width);
			Assert.AreEqual(5, result.Request.Height);
		}

		[Test]
		public void TestAspectFillSizingWithConstrainedHeight()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			image.Aspect = Aspect.AspectFill;
			var result = image.Measure(double.PositiveInfinity, 10);

			Assert.AreEqual(50, result.Request.Width);
			Assert.AreEqual(10, result.Request.Height);
		}

		[Test]
		public void TestAspectFillSizingWithConstrainedWidth()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			image.Aspect = Aspect.AspectFill;
			var result = image.Measure(25, double.PositiveInfinity);

			Assert.AreEqual(25, result.Request.Width);
			Assert.AreEqual(5, result.Request.Height);
		}

		[Test]
		public void TestFillSizingWithConstrainedHeight()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			image.Aspect = Aspect.AspectFill;
			var result = image.Measure(double.PositiveInfinity, 10);

			Assert.AreEqual(50, result.Request.Width);
			Assert.AreEqual(10, result.Request.Height);
		}

		[Test]
		public void TestFillSizingWithConstrainedWidth()
		{
			var image = new Image { Source = ImageSource.FromFile("File.png"), IsPlatformEnabled = true };

			image.Aspect = Aspect.AspectFill;
			var result = image.Measure(25, double.PositiveInfinity);

			Assert.AreEqual(25, result.Request.Width);
			Assert.AreEqual(5, result.Request.Height);
		}

		[Test]
		public void TestSizeChanged()
		{
			var image = new Image { Source = "File0.png" };
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
			var image = new Image();

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
			var image = new Image { Source = source };
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
			var image = new Image { Source = source };
			bool fired = false;
			image.MeasureInvalidated += (sender, e) => fired = true;
			Assert.Null(source.Stream);
			source.Stream = token => Task.FromResult<Stream>(null);
			Assert.NotNull(source.Stream);
			Assert.True(fired);
		}

		[Test]
		public void TestImageSourceToNullCancelsLoading()
		{
			var cancelled = false;

			var image = new Image();
			var mockImageRenderer = new MockImageRenderer(image);
			var loader = new StreamImageSource { Stream = GetStreamAsync };

			image.Source = loader;
			Assert.IsTrue(image.IsLoading);

			image.Source = null;
			mockImageRenderer.CompletionSource.Task.Wait();
			Assert.IsFalse(image.IsLoading);
			Assert.IsTrue(cancelled);

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
	}
}
