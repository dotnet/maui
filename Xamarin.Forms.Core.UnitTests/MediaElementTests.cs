using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class MediaElementTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
		}

		[Test]
		public void TestSource()
		{
			var mediaElement = new MediaElement();

			Assert.IsNull(mediaElement.Source);

			bool signaled = false;
			mediaElement.PropertyChanged += (sender, e) => {
				if (e.PropertyName == "Source")
					signaled = true;
			};

			var source = MediaSource.FromFile("Video.mp4");
			mediaElement.Source = source;

			Assert.AreEqual(source, mediaElement.Source);
			Assert.True(signaled);
		}

		[Test]
		public void TestSourceDoubleSet()
		{
			var mediaElement = new MediaElement { Source = MediaSource.FromFile("Video.mp4") };

			bool signaled = false;
			mediaElement.PropertyChanged += (sender, e) => {
				if (e.PropertyName == "Source")
					signaled = true;
			};

			mediaElement.Source = mediaElement.Source;

			Assert.False(signaled);
		}

		[Test]
		public void TestFileMediaSourceChanged()
		{
			var source = (FileMediaSource)MediaSource.FromFile("Video.mp4");

			bool signaled = false;
			source.SourceChanged += (sender, e) => {
				signaled = true;
			};

			source.File = "Other.mp4";
			Assert.AreEqual("Other.mp4", source.File);

			Assert.True(signaled);
		}


		[Test]
		public void TestSourceRoundTrip()
		{
			var uri = new Uri("https://sec.ch9.ms/ch9/5d93/a1eab4bf-3288-4faf-81c4-294402a85d93/XamarinShow_mid.mp4");
			var media = new MediaElement();
			Assert.Null(media.Source);
			media.Source = uri;
			Assert.NotNull(media.Source);
			Assert.IsInstanceOf(typeof(UriMediaSource), media.Source, "Not expected mediasource type");
			Assert.AreEqual(uri, ((UriMediaSource)media.Source).Uri);
		}
	}
}
