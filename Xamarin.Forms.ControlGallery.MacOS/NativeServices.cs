using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.MacOS;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.MacOS;

[assembly: Dependency(typeof(TestCloudService))]
[assembly: Dependency(typeof(StringProvider))]
[assembly: Dependency(typeof(CacheService))]
[assembly: ExportRenderer(typeof(DisposePage), typeof(DisposePageRenderer))]
[assembly: ExportRenderer(typeof(DisposeLabel), typeof(DisposeLabelRenderer))]

namespace Xamarin.Forms.ControlGallery.MacOS
{
	public class CacheService : ICacheService
	{
		public void ClearImageCache()
		{
			var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var cache = Path.Combine(documents, ".config", ".isolated-storage", "ImageLoaderCache");
			foreach (var file in Directory.GetFiles(cache))
			{
				File.Delete(file);
			}
		}
	}

	public class DisposePageRenderer : PageRenderer
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((DisposePage)Element).SendRendererDisposed();
			}
			base.Dispose(disposing);

		}
	}

	public class DisposeLabelRenderer : LabelRenderer
	{
		protected override void Dispose(bool disposing)
		{

			if (disposing)
			{
				((DisposeLabel)Element).SendRendererDisposed();
			}
			base.Dispose(disposing);
		}
	}

	public class StringProvider : IStringProvider
	{
		public string CoreGalleryTitle
		{
			get { return "iOS Core Gallery"; }
		}
	}

	public class TestCloudService : ITestCloudService
	{
		public bool IsOnTestCloud()
		{
			var isInTestCloud = Environment.GetEnvironmentVariable("XAMARIN_TEST_CLOUD");

			return isInTestCloud != null && isInTestCloud.Equals("1");
		}

		public string GetTestCloudDeviceName()
		{
			return Environment.GetEnvironmentVariable("XTC_DEVICE_NAME");
		}

		public string GetTestCloudDevice()
		{
			return Environment.GetEnvironmentVariable("XTC_DEVICE");
		}
	}
}

