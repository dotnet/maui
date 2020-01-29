using NUnit.Framework;
using System;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class MediaSourceTests : BaseTestFixture
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
			var filesource = new FileMediaSource { File = "File.mp4" };
			Assert.AreEqual("File.mp4", filesource.File);

			var urisource = new UriMediaSource { Uri = new Uri("http://xamarin.com/media.mp4") };
			Assert.AreEqual("http://xamarin.com/media.mp4", urisource.Uri.AbsoluteUri);
		}

		[Test]
		public void TestHelpers()
		{
			var mediasource = MediaSource.FromFile("File.mp4");
			Assert.That(mediasource, Is.TypeOf<FileMediaSource>());
			Assert.AreEqual("File.mp4", ((FileMediaSource)mediasource).File);

			var urisource = MediaSource.FromUri(new Uri("http://xamarin.com/media.mp4"));
			Assert.That(urisource, Is.TypeOf<UriMediaSource>());
			Assert.AreEqual("http://xamarin.com/media.mp4", ((UriMediaSource)(urisource)).Uri.AbsoluteUri);
		}

		[Test]
		public void TestImplicitFileConversion()
		{
			var mediaElement = new MediaElement { Source = "File.mp4" };
			Assert.IsTrue(mediaElement.Source != null);
			Assert.That(mediaElement.Source, Is.InstanceOf<FileMediaSource>());
			Assert.AreEqual("File.mp4", ((FileMediaSource)(mediaElement.Source)).File);
		}

		[Test]
		public void TestImplicitStringConversionWhenNull()
		{
			string s = null;
			var sut = (MediaSource)s;
			Assert.That(sut, Is.InstanceOf<FileMediaSource>());
			Assert.IsNull(((FileMediaSource)sut).File);
		}

		[Test]
		public void TestImplicitUriConversion()
		{
			var mediaElement = new MediaElement { Source = new Uri("http://xamarin.com/media.mp4") };
			Assert.IsTrue(mediaElement.Source != null);
			Assert.That(mediaElement.Source, Is.InstanceOf<UriMediaSource>());
			Assert.AreEqual("http://xamarin.com/media.mp4", ((UriMediaSource)(mediaElement.Source)).Uri.AbsoluteUri);
		}

		[Test]
		public void TestImplicitStringUriConversion()
		{
			var mediaElement = new MediaElement { Source = "http://xamarin.com/media.mp4" };
			Assert.IsTrue(mediaElement.Source != null);
			Assert.That(mediaElement.Source, Is.InstanceOf<UriMediaSource>());
			Assert.AreEqual("http://xamarin.com/media.mp4", ((UriMediaSource)(mediaElement.Source)).Uri.AbsoluteUri);
		}

		[Test]
		public void TestImplicitUriConversionWhenNull()
		{
			Uri u = null;
			var sut = (MediaSource)u;
			Assert.IsNull(sut);
		}

		[Test]
		public void TestSetStringValue()
		{
			var mediaElement = new MediaElement();
			mediaElement.SetValue(MediaElement.SourceProperty, "media.mp4");
			Assert.IsNotNull(mediaElement.Source);
			Assert.That(mediaElement.Source, Is.InstanceOf<FileMediaSource>());
			Assert.AreEqual("media.mp4", ((FileMediaSource)(mediaElement.Source)).File);
		}

		[Test]
		public void TextBindToStringValue()
		{
			var mediaElement = new MediaElement();
			mediaElement.SetBinding(MediaElement.SourceProperty, ".");
			Assert.IsNull(mediaElement.Source);
			mediaElement.BindingContext = "media.mp4";
			Assert.IsNotNull(mediaElement.Source);
			Assert.That(mediaElement.Source, Is.InstanceOf<FileMediaSource>());
			Assert.AreEqual("media.mp4", ((FileMediaSource)(mediaElement.Source)).File);
		}

		[Test]
		public void TextBindToStringUriValue()
		{
			var mediaElement = new MediaElement();
			mediaElement.SetBinding(MediaElement.SourceProperty, ".");
			Assert.IsNull(mediaElement.Source);
			mediaElement.BindingContext = "http://xamarin.com/media.mp4";
			Assert.IsNotNull(mediaElement.Source);
			Assert.That(mediaElement.Source, Is.InstanceOf<UriMediaSource>());
			Assert.AreEqual("http://xamarin.com/media.mp4", ((UriMediaSource)(mediaElement.Source)).Uri.AbsoluteUri);
		}

		[Test]
		public void TextBindToUriValue()
		{
			var mediaElement = new MediaElement();
			mediaElement.SetBinding(MediaElement.SourceProperty, ".");
			Assert.IsNull(mediaElement.Source);
			mediaElement.BindingContext = new Uri("http://xamarin.com/media.mp4");
			Assert.IsNotNull(mediaElement.Source);
			Assert.That(mediaElement.Source, Is.InstanceOf<UriMediaSource>());
			Assert.AreEqual("http://xamarin.com/media.mp4", ((UriMediaSource)(mediaElement.Source)).Uri.AbsoluteUri);
		}

		class MockMediaSource : MediaSource
		{
		}

		[Test]
		public void TestBindingContextPropagation()
		{
			var context = new object();
			var mediaElement = new MediaElement();
			mediaElement.BindingContext = context;
			var source = new MockMediaSource();
			mediaElement.Source = source;
			Assert.AreSame(context, source.BindingContext);

			mediaElement = new MediaElement();
			source = new MockMediaSource();
			mediaElement.Source = source;
			mediaElement.BindingContext = context;
			Assert.AreSame(context, source.BindingContext);
		}

		[Test]
		public void ImplicitCastOnAbsolutePathsShouldCreateAFileMediaSource()
		{
			var path = "/private/var/mobile/Containers/Data/Application/B1E5AB19-F815-4B4A-AB97-BD4571D53743/Documents/temp/video.mp4";
			var mediaElement = new MediaElement { Source = path };
			Assert.That(mediaElement.Source, Is.TypeOf<FileMediaSource>());
		}

		[Test]
		public void ImplicitCastOnWindowsAbsolutePathsShouldCreateAFileMediaSource()
		{
			var path = "C:\\Users\\Username\\Videos\\video.mp4";
			var mediaElement = new MediaElement { Source = path };
			Assert.That(mediaElement.Source, Is.TypeOf<FileMediaSource>());
		}
	}
}
