#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using ElmSharp;
using Tizen.UIExtensions.ElmSharp;
using DPExtensions = Microsoft.Maui.Platform.DPExtensions;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class ItemTemplateAdaptor : ItemAdaptor
	{
		Dictionary<EvasObject, View> _nativeTable = new Dictionary<EvasObject, View>();
		Dictionary<object, View?> _dataBindedViewTable = new Dictionary<object, View?>();
		protected View? _headerCache;
		protected View? _footerCache;
		IMauiContext _context;

		public ItemTemplateAdaptor(ItemsView itemsView) : this(itemsView, itemsView.ItemsSource, itemsView.ItemTemplate) { }

		protected ItemTemplateAdaptor(ItemsView itemsView, IEnumerable items, DataTemplate template) : base(items)
		{
			ItemTemplate = template;
			Element = itemsView;
			IsSelectable = itemsView is SelectableItemsView;
			_context = itemsView.Handler!.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
		}

		protected DataTemplate ItemTemplate { get; set; }

		protected Element Element { get; set; }

		protected virtual bool IsSelectable { get; }


		public View GetTemplatedView(EvasObject evasObject)
		{
			return _nativeTable[evasObject];
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

		public override EvasObject? CreateNativeView(EvasObject parent)
		{
			return CreateNativeView(0, parent);
		}

		public override EvasObject? CreateNativeView(int index, EvasObject parent)
		{
			View? view = null;
			if (ItemTemplate is DataTemplateSelector selector)
			{
				view = selector.SelectTemplate(this[index], Element).CreateContent() as View;
			}
			else
			{
				view = ItemTemplate.CreateContent() as View;
			}

			if (view != null)
			{
				var native = view.ToPlatform(_context);
				view.Parent = Element;
				_nativeTable[native] = view;
				return native;
			}
			return null;
		}

		public override EvasObject? GetFooterView(EvasObject parent)
		{
			_footerCache = CreateFooterView();
			if (_footerCache != null)
			{
				_footerCache.Parent = Element;
				return _footerCache.ToPlatform(_context);
			}
			return null;
		}

		public override EvasObject? GetHeaderView(EvasObject parent)
		{
			_headerCache = CreateHeaderView();
			if (_headerCache != null)
			{
				_headerCache.Parent = Element;
				return _headerCache.ToPlatform(_context);
			}
			return null;
		}

		public override Size MeasureFooter(int widthConstraint, int heightConstraint)
		{
			return _footerCache?.Measure(DPExtensions.ConvertToScaledDP(widthConstraint), DPExtensions.ConvertToScaledDP(heightConstraint)).Request.ToEFLPixel() ?? new Size(0, 0);
		}

		public override Size MeasureHeader(int widthConstraint, int heightConstraint)
		{
			return _headerCache?.Measure(DPExtensions.ConvertToScaledDP(widthConstraint), DPExtensions.ConvertToScaledDP(heightConstraint)).Request.ToEFLPixel() ?? new Size(0, 0);
		}

		public override Size MeasureItem(int widthConstraint, int heightConstraint)
		{
			return MeasureItem(0, widthConstraint, heightConstraint);
		}

		public override Size MeasureItem(int index, int widthConstraint, int heightConstraint)
		{
			if (widthConstraint > heightConstraint)
			{
				widthConstraint = int.MaxValue;
			}
			if (heightConstraint > widthConstraint)
			{
				heightConstraint = int.MaxValue;
			}

			var item = this[index];
			if (item != null && _dataBindedViewTable.TryGetValue(item, out View? createdView) && createdView != null)
			{
				return createdView.Measure(DPExtensions.ConvertToScaledDP(widthConstraint), DPExtensions.ConvertToScaledDP(heightConstraint), MeasureFlags.IncludeMargins).Request.ToEFLPixel();
			}

			View? view = null;
			if (ItemTemplate is DataTemplateSelector selector)
			{
				view = selector.SelectTemplate(this[index], Element).CreateContent() as View;
			}
			else
			{
				view = ItemTemplate.CreateContent() as View;
			}

			if (view != null)
			{
				view.Parent = Element;
				if (Count > index)
					view.BindingContext = this[index];
				var request = view.Measure(DPExtensions.ConvertToScaledDP(widthConstraint), DPExtensions.ConvertToScaledDP(heightConstraint), MeasureFlags.IncludeMargins).Request;
				return request.ToEFLPixel();
			}
			return new Size(0, 0);
		}

		public override void RemoveNativeView(EvasObject native)
		{
			UnBinding(native);
			if (_nativeTable.TryGetValue(native, out View? view))
			{
				native.Unrealize();
				_nativeTable.Remove(native);
			}
		}

		public override void SetBinding(EvasObject native, int index)
		{
			if (_nativeTable.TryGetValue(native, out View? view))
			{
				ResetBindedView(view);
				var item = this[index];
				if (item != null)
				{
					view.BindingContext = item;
					_dataBindedViewTable[item] = view;
				}
				view.MeasureInvalidated += OnItemMeasureInvalidated;
				AddLogicalChild(view);
			}
		}

		public override void UnBinding(EvasObject native)
		{
			if (_nativeTable.TryGetValue(native, out View? view))
			{
				view.MeasureInvalidated -= OnItemMeasureInvalidated;
				ResetBindedView(view);
			}
		}

		public void SendItemSelected(object selectedItem)
		{
			if (Element is SelectableItemsView selectable)
			{
				selectable.SelectedItem = selectedItem;
			}
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
						header = structuredItemsView.HeaderTemplate.CreateContent() as View;
						if (header != null)
							header.BindingContext = structuredItemsView.Header;
					}
					else if (structuredItemsView.Header is String str)
					{
						header = new Label { Text = str, };
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
						footer = structuredItemsView.FooterTemplate.CreateContent() as View;
						if (footer != null)
							footer.BindingContext = structuredItemsView.Footer;
					}
					else if (structuredItemsView.Footer is String str)
					{
						footer = new Label { Text = str, };
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

		void OnItemMeasureInvalidated(object? sender, EventArgs e)
		{
			var data = (sender as View)?.BindingContext ?? null;
			if (data != null)
			{
				int index = GetItemIndex(data);
				if (index != -1)
				{
					CollectionView?.ItemMeasureInvalidated(index);
				}
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
