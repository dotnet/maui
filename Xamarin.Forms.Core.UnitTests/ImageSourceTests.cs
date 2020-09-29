using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ImageSourceTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[Test]
		public void TestConstructors()
		{
			var filesource = new FileImageSource { File = "File.png" };
			Assert.AreEqual("File.png", filesource.File);

			Func<CancellationToken, Task<Stream>> stream = token => new Task<Stream>(() => new FileStream("Foo", System.IO.FileMode.Open), token);
			var streamsource = new StreamImageSource { Stream = stream };
			Assert.AreEqual(stream, streamsource.Stream);
		}

		[Test]
		public void TestHelpers()
		{
			var imagesource = ImageSource.FromFile("File.png");
			Assert.That(imagesource, Is.TypeOf<FileImageSource>());
			Assert.AreEqual("File.png", ((FileImageSource)imagesource).File);

			Func<Stream> stream = () => new System.IO.FileStream("Foo", System.IO.FileMode.Open);
			var streamsource = ImageSource.FromStream(stream);
			Assert.That(streamsource, Is.TypeOf<StreamImageSource>());

			var urisource = ImageSource.FromUri(new Uri("http://xamarin.com/img.png"));
			Assert.That(urisource, Is.TypeOf<UriImageSource>());
			Assert.AreEqual("http://xamarin.com/img.png", ((UriImageSource)(urisource)).Uri.AbsoluteUri);
		}

		[Test]
		public void TestImplicitFileConversion()
		{
			var image = new Image { Source = "File.png" };
			Assert.IsTrue(image.Source != null);
			Assert.That(image.Source, Is.InstanceOf<FileImageSource>());
			Assert.AreEqual("File.png", ((FileImageSource)(image.Source)).File);
		}

		[Test]
		public void TestImplicitStringConversionWhenNull()
		{
			string s = null;
			var sut = (ImageSource)s;
			Assert.That(sut, Is.InstanceOf<FileImageSource>());
			Assert.IsNull(((FileImageSource)sut).File);
		}

		[Test]
		public void TestImplicitUriConversion()
		{
			var image = new Image { Source = new Uri("http://xamarin.com/img.png") };
			Assert.IsTrue(image.Source != null);
			Assert.That(image.Source, Is.InstanceOf<UriImageSource>());
			Assert.AreEqual("http://xamarin.com/img.png", ((UriImageSource)(image.Source)).Uri.AbsoluteUri);
		}

		[Test]
		public void TestImplicitStringUriConversion()
		{
			var image = new Image { Source = "http://xamarin.com/img.png" };
			Assert.IsTrue(image.Source != null);
			Assert.That(image.Source, Is.InstanceOf<UriImageSource>());
			Assert.AreEqual("http://xamarin.com/img.png", ((UriImageSource)(image.Source)).Uri.AbsoluteUri);
		}

		[Test]
		public void TestImplicitUriConversionWhenNull()
		{
			Uri u = null;
			var sut = (ImageSource)u;
			Assert.IsNull(sut);
		}

		[Test]
		public void TestSetStringValue()
		{
			var image = new Image();
			image.SetValue(Image.SourceProperty, "foo.png");
			Assert.IsNotNull(image.Source);
			Assert.That(image.Source, Is.InstanceOf<FileImageSource>());
			Assert.AreEqual("foo.png", ((FileImageSource)(image.Source)).File);
		}

		[Test]
		public void TextBindToStringValue()
		{
			var image = new Image();
			image.SetBinding(Image.SourceProperty, ".");
			Assert.IsNull(image.Source);
			image.BindingContext = "foo.png";
			Assert.IsNotNull(image.Source);
			Assert.That(image.Source, Is.InstanceOf<FileImageSource>());
			Assert.AreEqual("foo.png", ((FileImageSource)(image.Source)).File);
		}

		[Test]
		public void TextBindToStringUriValue()
		{
			var image = new Image();
			image.SetBinding(Image.SourceProperty, ".");
			Assert.IsNull(image.Source);
			image.BindingContext = "http://xamarin.com/img.png";
			Assert.IsNotNull(image.Source);
			Assert.That(image.Source, Is.InstanceOf<UriImageSource>());
			Assert.AreEqual("http://xamarin.com/img.png", ((UriImageSource)(image.Source)).Uri.AbsoluteUri);
		}

		[Test]
		public void TextBindToUriValue()
		{
			var image = new Image();
			image.SetBinding(Image.SourceProperty, ".");
			Assert.IsNull(image.Source);
			image.BindingContext = new Uri("http://xamarin.com/img.png");
			Assert.IsNotNull(image.Source);
			Assert.That(image.Source, Is.InstanceOf<UriImageSource>());
			Assert.AreEqual("http://xamarin.com/img.png", ((UriImageSource)(image.Source)).Uri.AbsoluteUri);
		}

		class MockImageSource : ImageSource
		{
		}

		[Test]
		public void TestBindingContextPropagation()
		{
			var context = new object();
			var image = new Image();
			image.BindingContext = context;
			var source = new MockImageSource();
			image.Source = source;
			Assert.AreSame(context, source.BindingContext);

			image = new Image();
			source = new MockImageSource();
			image.Source = source;
			image.BindingContext = context;
			Assert.AreSame(context, source.BindingContext);
		}

		[Test]
		public void ImplicitCastOnAbsolutePathsShouldCreateAFileImageSource()
		{
			var path = "/private/var/mobile/Containers/Data/Application/B1E5AB19-F815-4B4A-AB97-BD4571D53743/Documents/temp/IMG_20140603_150614_preview.jpg";
			var image = new Image { Source = path };
			Assert.That(image.Source, Is.TypeOf<FileImageSource>());
		}
	}
}