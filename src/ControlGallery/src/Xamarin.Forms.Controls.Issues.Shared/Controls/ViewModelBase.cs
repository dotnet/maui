using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	public class PropertyChangedBase : INotifyPropertyChanged
	{
		Dictionary<string, object> _properties = new Dictionary<string, object>();

		protected T GetProperty<T>([CallerMemberName] string name = null)
		{
			object value = null;
			if (_properties.TryGetValue(name, out value))
			{
				return value == null ? default(T) : (T)value;
			}
			return default(T);
		}

		protected void SetProperty<T>(T value, [CallerMemberName] string name = null)
		{
			if (Equals(value, GetProperty<T>(name)))
			{
				return;
			}
			_properties[name] = value;
			OnPropertyChanged(name);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	public class ViewModelError
	{
		public ViewModelError(string text)
		{
			Text = text;
		}

		public string Text { get; set; }

		public override bool Equals(object obj)
		{
			var error = obj as ViewModelError;
			if (error == null)
			{
				return false;
			}
			return Text.Equals(error.Text);
		}

		public override int GetHashCode()
		{
			return Text.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("ViewModelError: {0}", Text);
		}
	}

	public class ViewModelBase : PropertyChangedBase
	{
		public ViewModelBase()
		{
			_errors = new List<ViewModelError>();
			Validate();
		}

		readonly List<ViewModelError> _errors;

		public virtual bool IsValid
		{
			get { return _errors.Count <= 0; }
		}

		protected IEnumerable<ViewModelError> Errors
		{
			get { return _errors; }
		}

		public event EventHandler IsValidChanged;

		public event EventHandler IsBusyChanged;

		protected virtual void Validate()
		{
			OnPropertyChanged("IsValid");
			OnPropertyChanged("Errors");

			var callback = IsValidChanged;
			if (callback != null)
			{
				callback(this, EventArgs.Empty);
			}

			// Spit out errors for easier debugging.
			if (_errors != null && _errors.Count > 0)
			{
				Debug.WriteLine("Errors:");
				foreach (var error in _errors)
				{
					Debug.WriteLine(error);
				}
			}
		}

		protected virtual void ValidateProperty(Func<bool> validate, ViewModelError error)
		{
			if (validate())
			{
				_errors.Remove(error);
			}
			else if (!_errors.Contains(error))
			{
				_errors.Add(error);
			}
		}

		public virtual bool IsBusy
		{
			get { return _isBusy; }
			set
			{
				if (_isBusy != value)
				{
					_isBusy = value;
					OnPropertyChanged("IsBusy");
					OnIsBusyChanged();
				}
			}
		}

		bool _isBusy = false;

		protected virtual void OnIsBusyChanged()
		{
			// Some models might want to have a validation thet depends on the busy state.
			Validate();
			var method = IsBusyChanged;
			if (method != null)
				IsBusyChanged(this, EventArgs.Empty);
		}
	}

	public class DelegateCommand : ICommand
	{
		readonly Predicate<object> _canExecute;
		readonly Action<object> _execute;

		public event EventHandler CanExecuteChanged;

		public DelegateCommand(Action<object> execute)
			: this(execute, null)
		{
		}

		public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			if (_canExecute == null)
			{
				return true;
			}

			return _canExecute(parameter);
		}

		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		public void RaiseCanExecuteChanged()
		{
			var handler = CanExecuteChanged;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}
	}

	[Preserve(AllMembers = true)]
	public class ExampleViewModel : ViewModelBase
	{
		[Preserve(AllMembers = true)]
		public class Job : ViewModelBase
		{

			public string JobId
			{
				get { return GetProperty<string>(); }
				set { SetProperty(value); }
			}

			public string JobName
			{
				get { return GetProperty<string>(); }
				set { SetProperty(value); }
			}

			public double? Hours
			{
				get { return GetProperty<double?>(); }
				set { SetProperty(value); }
			}

			public bool Locked
			{
				get { return GetProperty<bool>(); }
				set { SetProperty(value); }
			}
		}

		public ExampleViewModel()
		{

			Jobs = new ObservableCollection<Job>()
			{
				new Job() { JobId = "3672", JobName = "Big Job", Hours = 2},
				new Job() { JobId = "6289", JobName = "Smaller Job", Hours = 2},
				new Job() { JobId = "3672-41", JobName = "Add On Job", Hours = 23},
			};
		}

		public ObservableCollection<Job> Jobs { get; set; }


		public ICommand AddOneCommand
		{
			get
			{
				if (_addOneCommand == null)
				{
					_addOneCommand = new DelegateCommand(obj =>
					{
						Jobs.Add(new Job() { JobId = "1234", JobName = "add one", Hours = 12 });
					}, obj => !IsBusy);
				}
				return _addOneCommand;
			}
		}

		ICommand _addOneCommand;

		public ICommand AddTwoCommand
		{
			get
			{
				if (_addTwoCommand == null)
				{
					_addTwoCommand = new DelegateCommand(obj =>
					{
						Jobs.Add(new Job() { JobId = "9999", JobName = "add two", Hours = 12 });
						Jobs.Add(new Job() { JobId = "8888", JobName = "add two", Hours = 12 });
					}, obj => !IsBusy);
				}
				return _addTwoCommand;
			}
		}

		ICommand _addTwoCommand;

		public void GetHours()
		{

			var results = new ObservableCollection<Job>()
			{
				new Job() { JobId = "3672", JobName = "RADO", Hours = 2},
				new Job() { JobId = "6289", JobName = "MGA Life Cycle Flexible Test System", Hours = 2},

			};

			foreach (var x in results)
				Jobs.Add(x);

		}
	}

	public class DoubleStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (null == value || "0" == value.ToString())
				return string.Empty;
			return value.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			double? returnValue = null;
			double convertResult;
			var strvalue = value as string;
			if (double.TryParse(strvalue, out convertResult))
			{
				returnValue = convertResult;
			}
			return returnValue;
		}
	}


}