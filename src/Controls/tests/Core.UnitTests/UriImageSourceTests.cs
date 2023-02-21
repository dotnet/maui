using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class UriImageSourceTests : BaseTestFixture
	{

		public UriImageSourceTests()
		{

			networkcalls = 0;
		}

		static Random rnd = new Random();
		static int networkcalls = 0;
		static async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			await Task.Delay(rnd.Next(30, 2000));
			if (cancellationToken.IsCancellationRequested)
				throw new TaskCanceledException();
			networkcalls++;
			return typeof(UriImageSourceTests).Assembly.GetManifestResourceStream(uri.LocalPath.Substring(1));
		}

		[Fact(Skip = "LoadImageFromStream")]
		public void LoadImageFromStream()
		{
			IStreamImageSource loader = new UriImageSource
			{
				Uri = new Uri("http://foo.com/Images/crimson.jpg"),
			};
			Stream s0 = loader.GetStreamAsync().Result;

			Assert.Equal(79109, s0.Length);
		}

		[Fact(Skip = "SecondCallLoadFromCache")]
		public void SecondCallLoadFromCache()
		{
			IStreamImageSource loader = new UriImageSource
			{
				Uri = new Uri("http://foo.com/Images/crimson.jpg"),
			};
			Assert.Equal(0, networkcalls);

			using (var s0 = loader.GetStreamAsync().Result)
			{
				Assert.Equal(79109, s0.Length);
				Assert.Equal(1, networkcalls);
			}

			using (var s1 = loader.GetStreamAsync().Result)
			{
				Assert.Equal(79109, s1.Length);
				Assert.Equal(1, networkcalls);
			}
		}

		[Fact(Skip = "DoNotKeepFailedRetrieveInCache")]
		public void DoNotKeepFailedRetrieveInCache()
		{
			IStreamImageSource loader = new UriImageSource
			{
				Uri = new Uri("http://foo.com/missing.png"),
			};
			Assert.Equal(0, networkcalls);

			var s0 = loader.GetStreamAsync().Result;
			Assert.Null(s0);
			Assert.Equal(1, networkcalls);

			var s1 = loader.GetStreamAsync().Result;
			Assert.Null(s1);
			Assert.Equal(2, networkcalls);
		}

		[Fact(Skip = "ConcurrentCallsOnSameUriAreQueued")]
		public void ConcurrentCallsOnSameUriAreQueued()
		{
			IStreamImageSource loader = new UriImageSource
			{
				Uri = new Uri("http://foo.com/Images/crimson.jpg"),
			};
			Assert.Equal(0, networkcalls);

			var t0 = loader.GetStreamAsync();
			var t1 = loader.GetStreamAsync();

			//var s0 = t0.Result;
			using (var s1 = t1.Result)
			{
				Assert.Equal(1, networkcalls);
				Assert.Equal(79109, s1.Length);
			}
		}

		[Fact]
		public void NullUriDoesNotCrash()
		{
			var loader = new UriImageSource();
			loader.Uri = null;
		}

		[Fact]
		public void UrlHashKeyAreTheSame()
		{
			var urlHash1 = Crc64.ComputeHashString("http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasdasdasdasd");
			var urlHash2 = Crc64.ComputeHashString("http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasdasdasdasd");
			Assert.True(urlHash1 == urlHash2);
		}

		[Fact]
		public void UrlHashKeyAreNotTheSame()
		{
			var urlHash1 = Crc64.ComputeHashString("http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasdasdasdasd");
			var urlHash2 = Crc64.ComputeHashString("http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasda");
			Assert.True(urlHash1 != urlHash2);
		}

	}
}
