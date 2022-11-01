using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public interface IImageStub : IImage
	{
		public event Action LoadingStarted;
		public event Action<bool> LoadingCompleted;
		public event Action<Exception> LoadingFailed;
		new IImageSource Source { get; set; }
		bool IsLoading { get; }
		new bool IsAnimationPlaying { get; set; }
		new Aspect Aspect { get; set; }
	}
}
