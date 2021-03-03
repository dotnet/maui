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
		protected View _headerCache;
		protected View _footerCache;

		public ItemTemplateAdaptor(ItemsView itemsView) : this(itemsView, itemsView.ItemsSource, itemsView.ItemTemplate) { }

		protected ItemTemplateAdaptor(Element itemsView, IEnumerable items, DataTemplate template) : base(items)
		{
			ItemTemplate = template;
			Element = itemsView;
			IsSelectable = itemsView is SelectableItemsView;
		}

		protected DataTemplate ItemTemplate { get; set; }

		protected Element Element { get; set; }

		protected virtual bool IsSelectable { get; }

		public View GetTemplatedView(EvasObject evasObject)
		{
			return _nativeFormsTable[evasObject];
		}

		public View GetTemplatedView(int index)
		{

			if (Count > index && _dataBindedViewTable.TryGetValue(this[index], out View view))
			{
				return view;
			}
			return null;
		}

		public override object GetViewCategory(int index)
		{
			if (ItemTemplate is DataTemplateSelector selector)
			{
				return selector.SelectTemplate(this[index], Element);
			}
			return base.GetViewCategory(index);
		}

		public override EvasObject CreateNativeView(int index, EvasObject parent)
		{
			View view = null;
			if (ItemTemplate is DataTemplateSelector selector)
			{
				view = selector.SelectTemplate(this[index], Element).CreateContent() as View;
			}
			else
			{
				view = ItemTemplate.CreateContent() as View;
			}
			var renderer = Platform.GetOrCreateRenderer(view);
			var native = renderer.NativeView;

			view.Parent = Element;
			(renderer as ILayoutRenderer)?.RegisterOnLayoutUpdated();

			_nativeFormsTable[native] = view;
			return native;
		}

		public override EvasObject CreateNativeView(EvasObject parent)
		{
			return CreateNativeView(0, parent);
		}

		public override EvasObject GetHeaderView(EvasObject parent)
		{
			_headerCache = CreateHeaderView();
			if (_headerCache != null)
			{
				_headerCache.Parent = Element;
				var renderer = Platform.GetOrCreateRenderer(_headerCache);
				(renderer as ILayoutRenderer)?.RegisterOnLayoutUpdated();
				return renderer.NativeView;
			}
			return null;
		}

		public override EvasObject GetFooterView(EvasObject parent)
		{
			_footerCache = CreateFooterView();
			if (_footerCache != null)
			{
				_footerCache.Parent = Element;
				var renderer = Platform.GetOrCreateRenderer(_footerCache);
				(renderer as ILayoutRenderer)?.RegisterOnLayoutUpdated();
				return renderer.NativeView;
			}
			return null;
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
				AddLogicalChild(view);
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
				view = selector.SelectTemplate(this[index], Element).CreateContent() as View;
			}
			else
			{
				view = ItemTemplate.CreateContent() as View;
			}
			using (var renderer = Platform.GetOrCreateRenderer(view))
			{
				view.Parent = Element;
				if (Count > index)
					view.BindingContext = this[index];
				var request = view.Measure(Forms.ConvertToScaledDP(widthConstraint), Forms.ConvertToScaledDP(heightConstraint), MeasureFlags.IncludeMargins).Request;
				return request.ToPixel();
			}
		}

		public override ESize MeasureHeader(int widthConstraint, int heightConstraint)
		{
			return _headerCache?.Measure(Forms.ConvertToScaledDP(widthConstraint), Forms.ConvertToScaledDP(heightConstraint)).Request.ToPixel() ?? new ESize(0, 0);
		}

		public override ESize MeasureFooter(int widthConstraint, int heightConstraint)
		{
			return _footerCache?.Measure(Forms.ConvertToScaledDP(widthConstraint), Forms.ConvertToScaledDP(heightConstraint)).Request.ToPixel() ?? new ESize(0, 0);
		}

		public override void UpdateViewState(EvasObject view, ViewHolderState state)
		{
			base.UpdateViewState(view, state);
			if (_nativeFormsTable.TryGetValue(view, out View formsView))
			{
				switch (state)
				{
					case ViewHolderState.Focused:
						VisualStateManager.GoToState(formsView, VisualStateManager.CommonStates.Focused);
						formsView.SetValue(VisualElement.IsFocusedPropertyKey, true);
						break;
					case ViewHolderState.Normal:
						VisualStateManager.GoToState(formsView, VisualStateManager.CommonStates.Normal);
						formsView.SetValue(VisualElement.IsFocusedPropertyKey, false);
						break;
					case ViewHolderState.Selected:
						if (IsSelectable)
							VisualStateManager.GoToState(formsView, VisualStateManager.CommonStates.Selected);
						break;
				}
			}
		}

		protected virtual View CreateHeaderView()
		{
			if (Element is StructuredItemsView structuredItemsView)
			{
				if (structuredItemsView.Header != null)
				{
					View header = null;
					if (structuredItemsView.Header is View view)
					{
						header = view;
					}
					else if (structuredItemsView.HeaderTemplate != null)
					{
						header = structuredItemsView.HeaderTemplate.CreateContent() as View;
						header.BindingContext = structuredItemsView.Header;
					}
					else if (structuredItemsView.Header is String str)
					{
						header = new XLabel { Text = str, };
					}
					return header;
				}
			}
			return null;
		}

		protected virtual View CreateFooterView()
		{
			if (Element is StructuredItemsView structuredItemsView)
			{
				if (structuredItemsView.Footer != null)
				{
					View footer = null;
					if (structuredItemsView.Footer is View view)
					{
						footer = view;
					}
					else if (structuredItemsView.FooterTemplate != null)
					{
						footer = structuredItemsView.FooterTemplate.CreateContent() as View;
						footer.BindingContext = structuredItemsView.Footer;
					}
					else if (structuredItemsView.Footer is String str)
					{
						footer = new XLabel { Text = str, };
					}
					return footer;
				}
			}
			return null;
		}

		void ResetBindedView(View view)
		{
			if (view.BindingContext != null && _dataBindedViewTable.ContainsKey(view.BindingContext))
			{
				_dataBindedViewTable[view.BindingContext] = null;
				RemoveLogicalChild(view);
				view.BindingContext = null;
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

		void AddLogicalChild(Element element)
		{
			if (Element is ItemsView iv)
			{
				iv.AddLogicalChild(element);
			}
			else
			{
				element.Parent = Element;
			}
		}

		void RemoveLogicalChild(Element element)
		{
			if (Element is ItemsView iv)
			{
				iv.RemoveLogicalChild(element);
			}
			else
			{
				element.Parent = null;
			}
		}

	}
}
