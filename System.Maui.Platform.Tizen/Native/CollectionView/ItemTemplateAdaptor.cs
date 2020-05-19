using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using ElmSharp;
using ESize = ElmSharp.Size;
using XLabel = Xamarin.Forms.Label;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class ItemDefaultTemplateAdaptor : ItemTemplateAdaptor
	{
		class ToTextConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return value?.ToString() ?? string.Empty;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
		}

		public ItemDefaultTemplateAdaptor(ItemsView itemsView) : base(itemsView)
		{
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new XLabel
				{
					TextColor = Color.Black,
				};
				label.SetBinding(XLabel.TextProperty, new Binding(".", converter: new ToTextConverter()));

				return new StackLayout
				{
					BackgroundColor = Color.White,
					Padding = 30,
					Children =
					{
						label
					}
				};
			});
		}
	}

	public class ItemTemplateAdaptor : ItemAdaptor
	{
		Dictionary<EvasObject, View> _nativeFormsTable = new Dictionary<EvasObject, View>();
		Dictionary<object, View> _dataBindedViewTable = new Dictionary<object, View>();
		ItemsView _itemsView;

		public ItemTemplateAdaptor(ItemsView itemsView) : base(itemsView.ItemsSource)
		{
			ItemTemplate = itemsView.ItemTemplate;
			_itemsView = itemsView;
		}

		protected ItemTemplateAdaptor(ItemsView itemsView, IEnumerable items, DataTemplate template) : base(items)
		{
			ItemTemplate = template;
			_itemsView = itemsView;
		}

		protected DataTemplate ItemTemplate { get; set; }

		protected View GetTemplatedView(EvasObject evasObject)
		{
			return _nativeFormsTable[evasObject];
		}

		public override object GetViewCategory(int index)
		{
			if (ItemTemplate is DataTemplateSelector selector)
			{
				return selector.SelectTemplate(this[index], _itemsView);
			}
			return base.GetViewCategory(index);
		}

		public override EvasObject CreateNativeView(int index, EvasObject parent)
		{
			View view = null;
			if (ItemTemplate is DataTemplateSelector selector)
			{
				view = selector.SelectTemplate(this[index], _itemsView).CreateContent() as View;
			}
			else
			{
				view = ItemTemplate.CreateContent() as View;
			}
			var renderer = Platform.GetOrCreateRenderer(view);
			var native = Platform.GetOrCreateRenderer(view).NativeView;
			view.Parent = _itemsView;
			(renderer as LayoutRenderer)?.RegisterOnLayoutUpdated();

			_nativeFormsTable[native] = view;
			return native;
		}

		public override EvasObject CreateNativeView(EvasObject parent)
		{
			return CreateNativeView(0, parent);
		}

		public override void RemoveNativeView(EvasObject native)
		{
			UnBinding(native);
			if (_nativeFormsTable.TryGetValue(native, out View view))
			{
				Platform.GetRenderer(view)?.Dispose();
				_nativeFormsTable.Remove(native);
			}
		}

		public override void SetBinding(EvasObject native, int index)
		{
			if (_nativeFormsTable.TryGetValue(native, out View view))
			{
				ResetBindedView(view);
				view.BindingContext = this[index];
				_dataBindedViewTable[this[index]] = view;

				view.MeasureInvalidated += OnItemMeasureInvalidated;
			}
		}

		public override void UnBinding(EvasObject native)
		{
			if (_nativeFormsTable.TryGetValue(native, out View view))
			{
				view.MeasureInvalidated -= OnItemMeasureInvalidated;
				ResetBindedView(view);
			}
		}

		public override ESize MeasureItem(int widthConstraint, int heightConstraint)
		{
			return MeasureItem(0, widthConstraint, heightConstraint);
		}

		public override ESize MeasureItem(int index, int widthConstraint, int heightConstraint)
		{
			if (_dataBindedViewTable.TryGetValue(this[index], out View createdView) && createdView != null)
			{
				return createdView.Measure(Forms.ConvertToScaledDP(widthConstraint), Forms.ConvertToScaledDP(heightConstraint), MeasureFlags.IncludeMargins).Request.ToPixel();
			}

			View view = null;
			if (ItemTemplate is DataTemplateSelector selector)
			{
				view = selector.SelectTemplate(this[index], _itemsView).CreateContent() as View;
			}
			else
			{
				view = ItemTemplate.CreateContent() as View;
			}
			using (var renderer = Platform.GetOrCreateRenderer(view))
			{
				view.Parent = _itemsView;
				if (Count > index)
					view.BindingContext = this[index];
				var request = view.Measure(Forms.ConvertToScaledDP(widthConstraint), Forms.ConvertToScaledDP(heightConstraint), MeasureFlags.IncludeMargins).Request;
				return request.ToPixel();
			}
		}

		void ResetBindedView(View view)
		{
			if (view.BindingContext != null && _dataBindedViewTable.ContainsKey(view.BindingContext))
			{
				_dataBindedViewTable[view.BindingContext] = null;
			}
		}

		void OnItemMeasureInvalidated(object sender, EventArgs e)
		{
			var data = (sender as View)?.BindingContext ?? null;
			int index = GetItemIndex(data);
			if (index != -1)
			{
				CollectionView?.ItemMeasureInvalidated(index);
			}
		}
	}
}
