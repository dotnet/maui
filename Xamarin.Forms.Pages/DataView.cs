using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Forms.Pages
{
	public class DataView : ContentView, IDataSourceProvider
	{
		public static readonly BindableProperty DataProperty = BindableProperty.Create(nameof(Data), typeof(IEnumerable<IDataItem>), typeof(DataView), default(IEnumerable<IDataItem>));

		public static readonly BindableProperty DataSourceProperty = BindableProperty.Create(nameof(DataSource), typeof(IDataSource), typeof(DataView), null, propertyChanged: OnDataSourceChanged);

		public static readonly BindableProperty DefaultItemTemplateProperty = BindableProperty.Create(nameof(DefaultItemTemplate), typeof(DataTemplate), typeof(DataView), default(DataTemplate));

		readonly HashSet<string> _maskedKeys = new HashSet<string>();

		public DataView()
		{
			SetBinding(DataProperty, new Binding("DataSource.Data", source: this));
		}

		public IEnumerable<IDataItem> Data
		{
			get { return (IEnumerable<IDataItem>)GetValue(DataProperty); }
			set { SetValue(DataProperty, value); }
		}

		public DataTemplate DefaultItemTemplate
		{
			get { return (DataTemplate)GetValue(DefaultItemTemplateProperty); }
			set { SetValue(DefaultItemTemplateProperty, value); }
		}

		public IDataSource DataSource
		{
			get { return (IDataSource)GetValue(DataSourceProperty); }
			set { SetValue(DataSourceProperty, value); }
		}

		void IDataSourceProvider.MaskKey(string key)
		{
			_maskedKeys.Add(key);
			IDataSource dataSource = DataSource;
			if (dataSource != null && !dataSource.MaskedKeys.Contains(key))
			{
				dataSource.MaskKey(key);
			}
		}

		void IDataSourceProvider.UnmaskKey(string key)
		{
			_maskedKeys.Remove(key);
			DataSource?.UnmaskKey(key);
		}

		static void OnDataSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var dataView = (DataView)bindable;
			var dataSource = (IDataSource)newValue;
			var oldSource = (IDataSource)oldValue;

			if (oldSource != null)
			{
				foreach (string key in dataView._maskedKeys)
					oldSource.UnmaskKey(key);
			}

			if (dataSource != null)
			{
				foreach (string key in dataView._maskedKeys)
				{
					dataSource.MaskKey(key);
				}
			}
		}
	}
}