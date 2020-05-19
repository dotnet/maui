using System;

namespace Xamarin.Forms.Platform.Tizen
{
	public interface IGestureController
	{
		void SendStarted(View sender, object data);

		void SendMoved(View sender, object data);

		void SendCompleted(View sender, object data);

		void SendCanceled(View sender, object data);
	}
}