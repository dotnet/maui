using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Pages
{
	public abstract class BaseDataSource : IDataSource, INotifyPropertyChanged
	{
		readonly DataSourceList _dataSourceList = new DataSourceList();
		bool _initialized;
		bool _isLoading;

		public IReadOnlyList<IDataItem> Data
		{
			get
			{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				Initialize();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				return _dataSourceList;
			}
		}

		public bool IsLoading
		{
			get { return _isLoading; }
			set
			{
				if (_isLoading == value)
					return;
				_isLoading = value;
				OnPropertyChanged();
			}
		}

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		public object this[string key]
		{
			get
			{
				Initialize();
				return GetValue(key);
			}
			set
			{
				Initialize();
				if (SetValue(key, value))
					OnKeyChanged(key);
			}
		}
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

		IEnumerable<string> IDataSource.MaskedKeys => _dataSourceList.MaskedKeys;

		async void IDataSource.MaskKey(string key)
		{
			await Initialize();
			_dataSourceList.MaskKey(key);
		}

		async void IDataSource.UnmaskKey(string key)
		{
			await Initialize();
			_dataSourceList.UnmaskKey(key);
		}

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { PropertyChanged += value; }
			remove { PropertyChanged -= value; }
		}

		protected abstract Task<IList<IDataItem>> GetRawData();

		protected abstract object GetValue(string key);

		protected void OnPropertyChanged([CallerMemberName] string property = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

		protected abstract bool SetValue(string key, object value);

		async Task Initialize()
		{
			// Do this lazy because GetRawData is virtual and calling it in the ctor is therefor unfriendly
			if (_initialized)
				return;
			_initialized = true;
			IList<IDataItem> rawData = await GetRawData();
			if (!(rawData is INotifyCollectionChanged))
			{
				Log.Warning("Xamarin.Forms.Pages", "DataSource does not implement INotifyCollectionChanged, updates will not be reflected");
				rawData = rawData.ToList(); // Make a copy so we can be sure this list wont change out from under us
			}
			_dataSourceList.MainList = rawData;

			// Test if INPC("Item") is enough to trigger a full reset rather than triggering a new event for each key?
			foreach (IDataItem dataItem in rawData)
			{
				OnKeyChanged(dataItem.Name);
			}
		}

		void OnKeyChanged(string key)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"Item[{key}]"));
		}

		event PropertyChangedEventHandler PropertyChanged;
	}
}