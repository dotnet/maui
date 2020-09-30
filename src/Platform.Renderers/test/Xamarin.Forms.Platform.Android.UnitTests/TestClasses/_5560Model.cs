using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Forms.Platform.Android.UnitTests
{
	public class _5560Model : System.ComponentModel.INotifyPropertyChanged
	{
		string _text;
		int _textChangedCounter = 0;
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		TaskCompletionSource<bool> _testCompleted = new TaskCompletionSource<bool>();
		object _bindingContext = null;

		public _5560Model()
		{
			_bindingContext = this;
		}

		void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}

		public string Text
		{
			get => _text;
			set
			{
				_text = value;

				if (!TestCompleted)
					OnPropertyChanged(nameof(Text));

				_textChangedCounter++;
				if (_textChangedCounter > 100)
					MarkTestCompleted();
			}
		}

		public object BindingContext
		{
			get
			{
				return _bindingContext;
			}
		}

		public void MarkTestCompleted()
		{
			// because this model is reused by multiple controls it can sometimes cause a ping pong effect
			// where multiple controls are updating the model and then the model is re-updating those controls
			// which then re-update the model
			TestCompleted = true;
			_bindingContext = null;
			OnPropertyChanged(nameof(BindingContext));
			_testCompleted.SetResult(true);
		}

		bool TestCompleted { get; set; }

		public Task WaitForTestToComplete()
		{
			return Task.WhenAny( new Task[] { _testCompleted.Task, Task.Delay(3000) });
		}
	}
}
