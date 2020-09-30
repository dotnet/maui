using System;
using System.ComponentModel;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	public class PanGestureHandler : GestureHandler
	{
		int _currentPanGestureId;

		public PanGestureHandler(IGestureRecognizer recognizer) : base(recognizer)
		{
		}

		public override GestureLayer.GestureType Type
		{
			get
			{
				return GestureLayer.GestureType.Momentum;
			}
		}

		protected override void OnStarted(View sender, object data)
		{
			_currentPanGestureId++;
			(Recognizer as IPanGestureController)?.SendPanStarted(sender, _currentPanGestureId);
		}

		protected override void OnMoved(View sender, object data)
		{
			var lineData = (GestureLayer.MomentumData)data;
			(Recognizer as IPanGestureController)?.SendPan(sender, Forms.ConvertToScaledDP(lineData.X2 - lineData.X1), Forms.ConvertToScaledDP(lineData.Y2 - lineData.Y1), _currentPanGestureId);
		}

		protected override void OnCompleted(View sender, object data)
		{
			(Recognizer as IPanGestureController)?.SendPanCompleted(sender, _currentPanGestureId);
		}

		protected override void OnCanceled(View sender, object data)
		{
			(Recognizer as IPanGestureController)?.SendPanCanceled(sender, _currentPanGestureId);
		}
	}
}