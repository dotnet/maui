using System.Collections.Generic;
using WImage = Microsoft.UI.Xaml.Controls.Image;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CountedImageHandler
	{
		protected override WImage CreatePlatformView()
		{
			var image = new WImage();
			image.RegisterPropertyChangedCallback(WImage.SourceProperty, (s, e) => Log(image.Source, "Source"));
			return image;
		}

		public List<(string Member, object Value)> ImageEvents { get; } = new List<(string, object)>();

		void Log(object value, string member)
		{
			ImageEvents.Add((member, value));
		}
	}
}
