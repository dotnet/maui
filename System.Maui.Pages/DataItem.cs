using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms.Pages
{
	public class DataItem : IDataItem, INotifyPropertyChanged
	{
		string _name;
		object _val;

		public DataItem()
		{
		}

		public DataItem(string name, object value)
		{
			_name = name;
			_val = value;
		}

		public string Name
		{
			get { return _name; }
			set
			{
				if (_name == value)
					return;
				_name = value;
				OnPropertyChanged();
			}
		}

		public object Value
		{
			get { return _val; }
			set
			{
				if (_val == value)
					return;
				_val = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((DataItem)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_name?.GetHashCode() ?? 0) * 397) ^ (_val?.GetHashCode() ?? 0);
			}
		}

		public static bool operator ==(DataItem left, DataItem right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(DataItem left, DataItem right)
		{
			return !Equals(left, right);
		}

		protected bool Equals(DataItem other)
		{
			return string.Equals(_name, other._name) && Equals(_val, other._val);
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}