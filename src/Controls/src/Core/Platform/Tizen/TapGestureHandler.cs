using NGestureDetector = Tizen.NUI.GestureDetector;
using NTapGestureDetector = Tizen.NUI.TapGestureDetector;

namespace Microsoft.Maui.Controls.Platform
{
	public class TapGestureHandler : GestureHandler
	{
		public TapGestureHandler(IGestureRecognizer recognizer) : base(recognizer)
		{
			NativeDetector.Detected += OnTapped;
		}

		new TapGestureRecognizer Recognizer => (TapGestureRecognizer)base.Recognizer;
		new NTapGestureDetector NativeDetector => (NTapGestureDetector)base.NativeDetector;

		protected override NGestureDetector CreateNativeDetector(IGestureRecognizer recognizer)
		{
			return new NTapGestureDetector((uint)Recognizer.NumberOfTapsRequired);
		}

		void OnTapped(object source, NTapGestureDetector.DetectedEventArgs e)
		{
			if (e.TapGesture.NumberOfTaps == Recognizer.NumberOfTapsRequired && View != null)
				Recognizer.SendTapped(View);
		}
	}
}