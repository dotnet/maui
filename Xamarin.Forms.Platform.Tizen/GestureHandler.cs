using System;
using System.ComponentModel;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	public abstract class GestureHandler : IGestureController, INotifyPropertyChanged, IRegisterable
	{
		public IGestureRecognizer Recognizer { get; private set; }

		public abstract GestureLayer.GestureType Type { get; }

		public virtual double Timeout { get; }

		protected GestureHandler(IGestureRecognizer recognizer)
		{
			Recognizer = recognizer;
			Recognizer.PropertyChanged += OnRecognizerPropertyChanged;
		}

		public virtual event PropertyChangedEventHandler PropertyChanged;

		protected abstract void OnStarted(View sender, object data);

		protected abstract void OnMoved(View sender, object data);

		protected abstract void OnCompleted(View sender, object data);

		protected abstract void OnCanceled(View sender, object data);

		void IGestureController.SendStarted(View sender, object data)
		{
			OnStarted(sender, data);
		}

		void IGestureController.SendCompleted(View sender, object data)
		{
			OnCompleted(sender, data);
		}

		void IGestureController.SendMoved(View sender, object data)
		{
			OnMoved(sender, data);
		}

		void IGestureController.SendCanceled(View sender, object data)
		{
			OnCanceled(sender, data);
		}

		protected virtual void OnRecognizerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}
	}
}