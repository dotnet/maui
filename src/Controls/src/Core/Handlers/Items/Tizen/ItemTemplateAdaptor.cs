using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Tizen.UIExtensions.NUI;
using NView = Tizen.NUI.BaseComponents.View;
using TSize = Tizen.UIExtensions.Common.Size;
using XLabel = Microsoft.Maui.Controls.Label;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class CollectionViewSelectionChangedEventArgs : EventArgs
	{
		public IList<object>? SelectedItems { get; set; }
	}

	public class ItemTemplateAdaptor : ItemAdaptor
	{
		Dictionary<NView, View> _nativeMauiTable = new Dictionary<NView, View>();
		Dictionary<object, View?> _dataBindedViewTable = new Dictionary<object, View?>();
		protected View? _headerCache;
		protected View? _footerCache;

		public ItemTemplateAdaptor(ItemsView itemsView) : this(itemsView, itemsView.ItemsSource, itemsView.ItemTemplate ?? new DefaultItemTemplate()) { }

		protected ItemTemplateAdaptor(Element itemsView, IEnumerable items, DataTemplate template) : base(items)
		{
			ItemTemplate = template;
			Element = itemsView;
			IsSelectable = itemsView is SelectableItemsView;
		}

		public event EventHandler<CollectionViewSelectionChangedEventArgs>? SelectionChanged;

		protected DataTemplate ItemTemplate { get; set; }

		protected Element Element { get; set; }

		protected virtual bool IsSelectable { get; }

		protected IMauiContext MauiContext => Element.Handler!.MauiContext!;

		public object GetData(int index)
		{
			if (this[index] == null)
				throw new InvalidOperationException("No data");
			return this[index]!;
		}

		public override void SendItemSelected(IEnumerable<int> selected)
		{
			var items = new List<object>();
			foreach (var idx in selected)
			{
				if (idx < 0 || Count <= idx)
					continue;

				var selectedObject = this[idx];
				if (selectedObject != null)
					items.Add(selectedObject);
			}

			SelectionChanged?.Invoke(this, new CollectionViewSelectionChangedEventArgs
			{
				SelectedItems = items
			});
		}

		public override void UpdateViewState(NView view, ViewHolderState state)
		{
			base.UpdateViewState(view, state);
			if (_nativeMauiTable.TryGetValue(view, out View? formsView))
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

		public View? GetTemplatedView(NView view)
		{
			return _nativeMauiTable.ContainsKey(view) ? _nativeMauiTable[view] : null;
		}

		public View? GetTemplatedView(int index)
		{
			var item = this[index];
			if (item != null && Count > index && _dataBindedViewTable.TryGetValue(item, out View? view))
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

		public override NView CreateNativeView(int index)
		{
			View view;
			if (ItemTemplate is DataTemplateSelector selector)
			{
				view = (View)selector.SelectTemplate(GetData(index), Element).CreateContent();
			}
			else
			{
				view = (View)ItemTemplate.CreateContent();
			}
			var native = view.ToPlatform(MauiContext);
			_nativeMauiTable[native] = view;
			return native;
		}

		public override NView CreateNativeView()
		{
			return CreateNativeView(0);
		}

#pragma warning disable CS8764
		public override NView? GetHeaderView()
#pragma warning restore CS8764
		{
			if (_headerCache != null)
			{
				_headerCache.MeasureInvalidated -= OnHeaderFooterMeasureInvalidated;
			}
			_headerCache = CreateHeaderView();
			if (_headerCache != null)
			{
				_headerCache.Parent = Element;

				if (_headerCache.Handler is IPlatformViewHandler nativeHandler)
					nativeHandler.Dispose();
				_headerCache.Handler = null;
				_headerCache.MeasureInvalidated += OnHeaderFooterMeasureInvalidated;
				return _headerCache.ToPlatform(MauiContext);
			}
			return null;
		}

#pragma warning disable CS8764
		public override NView? GetFooterView()
#pragma warning restore CS8764
		{
			if (_footerCache != null)
			{
				_footerCache.MeasureInvalidated -= OnHeaderFooterMeasureInvalidated;
			}
			_footerCache = CreateFooterView();
			if (_footerCache != null)
			{
				_footerCache.Parent = Element;
				if (_footerCache.Handler is IPlatformViewHandler nativeHandler)
					nativeHandler.Dispose();
				_footerCache.Handler = null;
				_footerCache.MeasureInvalidated += OnHeaderFooterMeasureInvalidated;
				return _footerCache.ToPlatform(MauiContext);
			}
			return null;
		}

		public override void RemoveNativeView(NView native)
		{
			UnBinding(native);
			if (_nativeMauiTable.TryGetValue(native, out View? view))
			{
				if (view.Handler is IPlatformViewHandler handler)
				{
					_nativeMauiTable.Remove(handler.PlatformView!);
					handler.Dispose();
					view.Handler = null;
				}
			}
		}

		public override void SetBinding(NView native, int index)
		{
			if (_nativeMauiTable.TryGetValue(native, out View? view))
			{
				ResetBindedView(view);
				view.BindingContext = this[index];
				_dataBindedViewTable[this[index]!] = view;
				view.MeasureInvalidated += OnItemMeasureInvalidated;
				view.Parent = Element;

				AddLogicalChild(view);
			}
		}

		public override void UnBinding(NView native)
		{
			if (_nativeMauiTable.TryGetValue(native, out View? view))
			{
				view.MeasureInvalidated -= OnItemMeasureInvalidated;
				ResetBindedView(view);
			}
		}

		public override TSize MeasureItem(double widthConstraint, double heightConstraint)
		{
			return MeasureItem(0, widthConstraint, heightConstraint);
		}

		public override TSize MeasureItem(int index, double widthConstraint, double heightConstraint)
		{
			if (index < 0 || index >= Count || this[index] == null)
				return new TSize(0, 0);

			widthConstraint = widthConstraint.ToScaledDP();
			heightConstraint = heightConstraint.ToScaledDP();

			// TODO. It is hack code, it should be updated by Tizen.UIExtensions
			if (widthConstraint > heightConstraint)
				widthConstraint = double.PositiveInfinity;
			else
				heightConstraint = double.PositiveInfinity;

			if (_dataBindedViewTable.TryGetValue(GetData(index), out View? createdView) && createdView != null)
			{
				return (createdView as IView).Measure(widthConstraint, heightConstraint).ToPixel();
			}

			View view;
			if (ItemTemplate is DataTemplateSelector selector)
			{
				view = (View)selector.SelectTemplate(GetData(index), Element).CreateContent();
			}
			else
			{
				view = (View)ItemTemplate.CreateContent();
			}

			using var handler = (IPlatformViewHandler)view.Handler!;
			if (Count > index)
				view.BindingContext = this[index];
			view.Parent = Element;

			view.ToPlatform(MauiContext);
			return (view as IView).Measure(widthConstraint, heightConstraint).ToPixel();
		}

		public override TSize MeasureHeader(double widthConstraint, double heightConstraint)
		{
			// TODO. It is workaround code, if update Tizen.UIExtensions.NUI, this code will be removed
			if (CollectionView is Tizen.UIExtensions.NUI.CollectionView cv)
			{
				if (cv.LayoutManager != null)
				{
					if (cv.LayoutManager.IsHorizontal)
						widthConstraint = double.PositiveInfinity;
					else
						heightConstraint = double.PositiveInfinity;
				}
			}

			return (_headerCache as IView)?.Measure(widthConstraint.ToScaledDP(), heightConstraint.ToScaledDP()).ToPixel() ?? new TSize(0, 0);
		}

		public override TSize MeasureFooter(double widthConstraint, double heightConstraint)
		{
			return (_footerCache as IView)?.Measure(widthConstraint.ToScaledDP(), heightConstraint.ToScaledDP()).ToPixel() ?? new TSize(0, 0);
		}

		protected virtual View? CreateHeaderView()
		{
			if (Element is StructuredItemsView structuredItemsView)
			{
				if (structuredItemsView.Header != null)
				{
					View? header = null;
					if (structuredItemsView.Header is View view)
					{
						header = view;
					}
					else if (structuredItemsView.HeaderTemplate != null)
					{
						header = (View)structuredItemsView.HeaderTemplate.CreateContent();
						header.BindingContext = structuredItemsView.Header;
					}
					else if (structuredItemsView.Header is string str)
					{
						header = new XLabel { Text = str, };
					}
					return header;
				}
			}
			return null;
		}

		protected virtual View? CreateFooterView()
		{
			if (Element is StructuredItemsView structuredItemsView)
			{
				if (structuredItemsView.Footer != null)
				{
					View? footer = null;
					if (structuredItemsView.Footer is View view)
					{
						footer = view;
					}
					else if (structuredItemsView.FooterTemplate != null)
					{
						footer = (View)structuredItemsView.FooterTemplate.CreateContent();
						footer.BindingContext = structuredItemsView.Footer;
					}
					else if (structuredItemsView.Footer is string str)
					{
						footer = new XLabel { Text = str, };
					}
					return footer;
				}
			}
			return null;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_headerCache != null)
				{
					_headerCache.MeasureInvalidated -= OnHeaderFooterMeasureInvalidated;
				}
				if (_footerCache != null)
				{
					_footerCache.MeasureInvalidated -= OnHeaderFooterMeasureInvalidated;
				}
			}
			base.Dispose(disposing);
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

		void OnItemMeasureInvalidated(object? sender, EventArgs e)
		{
			var data = (sender as View)?.BindingContext ?? null;
			int index = data != null ? GetItemIndex(data) : -1;

			if (index != -1)
			{
				CollectionView?.ItemMeasureInvalidated(index);
			}
		}

		void OnHeaderFooterMeasureInvalidated(object? sender, EventArgs e)
		{
			CollectionView?.ItemMeasureInvalidated(-1);
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

	public class CarouselViewItemTemplateAdaptor : ItemTemplateAdaptor
	{
		public CarouselViewItemTemplateAdaptor(ItemsView itemsView) : base(itemsView) { }

		public override TSize MeasureItem(double widthConstraint, double heightConstraint)
		{
			return MeasureItem(0, widthConstraint, heightConstraint);
		}

		public override TSize MeasureItem(int index, double widthConstraint, double heightConstraint)
		{
			return (CollectionView as NView)!.Size.ToCommon();
		}
	}

	class DefaultItemTemplate : DataTemplate
	{
		public DefaultItemTemplate() : base(CreateView) { }

		class ToTextConverter : IValueConverter
		{
			public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
			{
				return value?.ToString() ?? string.Empty;
			}

			public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
		}

		static View CreateView()
		{
			var label = new XLabel
			{
				TextColor = Colors.Black,
			};
			label.SetBinding(XLabel.TextProperty, new Binding(".", converter: new ToTextConverter()));

			return new Controls.StackLayout
			{
				BackgroundColor = Colors.White,
				Padding = 30,
				Children =
					{
						label
					}
			};
		}
	}
}
