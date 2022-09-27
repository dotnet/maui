using System;
using NGestureDetector = Tizen.NUI.GestureDetector;
using TapGestureDetector = Tizen.NUI.TapGestureDetector;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class TapGestureHandler : GestureHandler
	{
		public TapGestureHandler(IGestureRecognizer recognizer) : base(recognizer)
		{
			NativeDetector.Detected += OnTapped;
		}

		new TapGestureRecognizer Recognizer => base.Recognizer as TapGestureRecognizer;
		new TapGestureDetector NativeDetector => base.NativeDetector as TapGestureDetector;

		protected override NGestureDetector CreateNativeDetector(IGestureRecognizer recognizer)
		{
			return new TapGestureDetector((uint)(recognizer as TapGestureRecognizer).NumberOfTapsRequired);
		}

		void OnTapped(object source, TapGestureDetector.DetectedEventArgs e)
		{
			if (e.TapGesture.NumberOfTaps == Recognizer.NumberOfTapsRequired)
				Recognizer.SendTapped(View);
		}

	}
}