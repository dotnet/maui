using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ImageSourceTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructors()
		{
			var filesource = new FileImageSource { File = "File.png" };
			Assert.Equal("File.png", filesource.File);

			Func<CancellationToken, Task<Stream>> stream = token => new Task<Stream>(() => new FileStream("Foo", System.IO.FileMode.Open), token);
			var streamsource = new StreamImageSource { Stream = stream };
			Assert.Equal(stream, streamsource.Stream);
		}

		[Fact]
		public void TestHelpers()
		{
			var imagesource = ImageSource.FromFile("File.png");
			Assert.IsType<FileImageSource>(imagesource);
			Assert.Equal("File.png", ((FileImageSource)imagesource).File);

			Func<Stream> stream = () => new System.IO.FileStream("Foo", System.IO.FileMode.Open);
			var streamsource = ImageSource.FromStream(stream);
			Assert.IsType<StreamImageSource>(streamsource);

			var urisource = ImageSource.FromUri(new Uri("http://xamarin.com/img.png"));
			Assert.IsType<UriImageSource>(urisource);
			Assert.Equal("http://xamarin.com/img.png", ((UriImageSource)(urisource)).Uri.AbsoluteUri);
		}

		[Fact]
		public void TestImplicitFileConversion()
		{
			var image = new Image { Source = "File.png" };
			Assert.True(image.Source != null);
			Assert.IsType<FileImageSource>(image.Source);
			Assert.Equal("File.png", ((FileImageSource)(image.Source)).File);
		}

		[Fact]
		public void TestImplicitStringConversionWhenNull()
		{
			string s = null;
			var sut = (ImageSource)s;
			Assert.IsType<FileImageSource>(sut);
			Assert.Null(((FileImageSource)sut).File);
		}

		[Fact]
		public void TestImplicitUriConversion()
		{
			var image = new Image { Source = new Uri("http://xamarin.com/img.png") };
			Assert.True(image.Source != null);
			Assert.IsType<UriImageSource>(image.Source);
			Assert.Equal("http://xamarin.com/img.png", ((UriImageSource)(image.Source)).Uri.AbsoluteUri);
		}

		[Fact]
		public void TestImplicitStringUriConversion()
		{
			var image = new Image { Source = "http://xamarin.com/img.png" };
			Assert.True(image.Source != null);
			Assert.IsType<UriImageSource>(image.Source);
			Assert.Equal("http://xamarin.com/img.png", ((UriImageSource)(image.Source)).Uri.AbsoluteUri);
		}

		[Fact]
		public void TestImplicitUriConversionWhenNull()
		{
			Uri u = null;
			var sut = (ImageSource)u;
			Assert.Null(sut);
		}

		[Fact]
		public void TestSetStringValue()
		{
			var image = new Image();
			image.SetValue(Image.SourceProperty, "foo.png");
			Assert.NotNull(image.Source);
			Assert.IsType<FileImageSource>(image.Source);
			Assert.Equal("foo.png", ((FileImageSource)(image.Source)).File);
		}

		[Fact]
		public void TextBindToStringValue()
		{
			var image = new Image();
			image.SetBinding(Image.SourceProperty, ".");
			Assert.Null(image.Source);
			image.BindingContext = "foo.png";
			Assert.NotNull(image.Source);
			Assert.IsType<FileImageSource>(image.Source);
			Assert.Equal("foo.png", ((FileImageSource)(image.Source)).File);
		}

		[Fact]
		public void TextBindToStringUriValue()
		{
			var image = new Image();
			image.SetBinding(Image.SourceProperty, ".");
			Assert.Null(image.Source);
			image.BindingContext = "http://xamarin.com/img.png";
			Assert.NotNull(image.Source);
			Assert.IsType<UriImageSource>(image.Source);
			Assert.Equal("http://xamarin.com/img.png", ((UriImageSource)(image.Source)).Uri.AbsoluteUri);
		}

		[Fact]
		public void TextBindToUriValue()
		{
			var image = new Image();
			image.SetBinding(Image.SourceProperty, ".");
			Assert.Null(image.Source);
			image.BindingContext = new Uri("http://xamarin.com/img.png");
			Assert.NotNull(image.Source);
			Assert.IsType<UriImageSource>(image.Source);
			Assert.Equal("http://xamarin.com/img.png", ((UriImageSource)(image.Source)).Uri.AbsoluteUri);
		}

		class MockImageSource : ImageSource
		{
		}

		[Fact]
		public void TestBindingContextPropagation()
		{
			var context = new object();
			var image = new Image();
			image.BindingContext = context;
			var source = new MockImageSource();
			image.Source = source;
			Assert.Same(context, source.BindingContext);

			image = new Image();
			source = new MockImageSource();
			image.Source = source;
			image.BindingContext = context;
			Assert.Same(context, source.BindingContext);
		}

		[Fact]
		public void ImplicitCastOnAbsolutePathsShouldCreateAFileImageSource()
		{
			var path = "/private/var/mobile/Containers/Data/Application/B1E5AB19-F815-4B4A-AB97-BD4571D53743/Documents/temp/IMG_20140603_150614_preview.jpg";
			var image = new Image { Source = path };
			Assert.IsType<FileImageSource>(image.Source);
		}

		[Fact]
		public async Task CancelCompletes()
		{
			var imageSource = new StreamImageSource
			{
				Stream = _ => Task.FromResult<Stream>(new MemoryStream())
			};
			await ((IStreamImageSource)imageSource).GetStreamAsync();
			await imageSource.Cancel(); // This should complete!
		}
	}
}
