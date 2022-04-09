using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CountedImageHandler
	{
		protected override UIImageView CreatePlatformView() => new CountedImageView();

		public List<(string Member, object Value)> ImageEvents => ((CountedImageView)PlatformView).ImageEvents;

		public class CountedImageView : UIImageView
		{
			public List<(string, object)> ImageEvents { get; } = new List<(string, object)>();

			public override UIImage Image
			{
				get => base.Image;
				set
				{
					base.Image = value;
					Log(value);
				}
			}

			void Log(object value, [CallerMemberName] string member = null)
			{
				ImageEvents.Add((member, value));
			}
		}
	}
}