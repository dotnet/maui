using System;
using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android
{
	public class EmptyViewAdapter : RecyclerView.Adapter
	{
		int _headerHeight;
		int _headerViewType;
		object _headerView;
		DataTemplate _headerViewTemplate;

		int _footerHeight;
		int _footerViewType;
		object _footerView;
		DataTemplate _footerViewTemplate;

		int _emptyItemViewType;
		object _emptyView;
		DataTemplate _emptyViewTemplate;

		public object Header
		{
			get => _headerView;
			set
			{
				_headerView = value;
				_headerViewType += 1;
				UpdateHeaderFooterHeight(_headerView, true);
			}
		}

		public DataTemplate HeaderTemplate
		{
			get => _headerViewTemplate;
			set
			{
				_headerViewTemplate = value;
				_headerViewType += 1;
				UpdateHeaderFooterHeight(_headerViewTemplate, true);
			}
		}

		public object Footer
		{
			get => _footerView;
			set
			{
				_footerView = value;
				_footerViewType += 1;
				UpdateHeaderFooterHeight(_footerView, false);
			}
		}

		public DataTemplate FooterTemplate
		{
			get => _footerViewTemplate;
			set
			{
				_footerViewTemplate = value;
				_footerViewType += 1;
				UpdateHeaderFooterHeight(_footerViewTemplate, false);
			}
		}


		public object EmptyView
		{
			get => _emptyView;
			set
			{
				_emptyView = value;

				// Change _itemViewType to force OnCreateViewHolder to run again and use this new EmptyView
				_emptyItemViewType += 1;
			}
		}

		public DataTemplate EmptyViewTemplate
		{
			get => _emptyViewTemplate;
			set
			{
				_emptyViewTemplate = value;

				// Change _itemViewType to force OnCreateViewHolder to run again and use this new template
				_emptyItemViewType += 1;
			}
		}

		protected readonly ItemsView ItemsView;
		public override int ItemCount => 1 + ((Header != null || HeaderTemplate != null) ? 1 : 0) + ((Footer != null || FooterTemplate != null) ? 1 : 0);

		public EmptyViewAdapter(ItemsView itemsView)
		{
			ItemsView = itemsView;

			_headerViewType = 1;
			_emptyItemViewType = 2;
			_footerViewType = 3;
		}

		public override void OnViewRecycled(Object holder)
		{
			if (holder is TemplatedItemViewHolder templatedItemViewHolder)
			{
				templatedItemViewHolder.Recycle(ItemsView);
			}
			else if (holder is SimpleViewHolder emptyViewHolder)
			{
				emptyViewHolder.Recycle(ItemsView);
			}

			base.OnViewRecycled(holder);
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			if (IsHeader(position))
			{
				if (holder is TemplatedItemViewHolder templatedItemViewHolder)
				{
					BindTemplatedItemViewHolder(templatedItemViewHolder, Header);
				}

				return;
			}

			if (IsFooter(position))
			{
				if (holder is TemplatedItemViewHolder templatedItemViewHolder)
				{
					BindTemplatedItemViewHolder(templatedItemViewHolder, Footer);
				}

				return;
			}

			if (IsEmpty(position))
			{
				if (holder is SimpleViewHolder emptyViewHolder && emptyViewHolder.View != null)
				{
					// For templated empty views, this will happen on bind. But if we just have a plain-old View,
					// we need to add it as a "child" of the ItemsView here so that stuff like Visual and FlowDirection
					// propagate to the controls in the EmptyView
					ItemsView.AddLogicalChild(emptyViewHolder.View);
				}
				else if (holder is TemplatedItemViewHolder templatedItemViewHolder && EmptyViewTemplate != null)
				{
					// Use EmptyView as the binding context for the template
					templatedItemViewHolder.Bind(EmptyView, ItemsView);
				}
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var context = parent.Context;

			if (viewType == _headerViewType)
			{
				return CreateHeaderFooterViewHolder(Header, HeaderTemplate, context);
			}

			if (viewType == _footerViewType)
			{
				return CreateHeaderFooterViewHolder(Footer, FooterTemplate, context);
			}

			if (viewType == _emptyItemViewType)
			{
				return CreateEmptyViewHolder(EmptyView, EmptyViewTemplate, parent);
			}

			return CreateEmptyViewHolder(EmptyView, EmptyViewTemplate, parent);
		}

		public override int GetItemViewType(int position)
		{
			if (IsHeader(position))
			{
				return _headerViewType;
			}

			if (IsFooter(position))
			{
				return _footerViewType;
			}

			if (IsEmpty(position))
			{
				return _emptyItemViewType;
			}

			return base.GetItemViewType(position);
		}

		protected RecyclerView.ViewHolder CreateHeaderFooterViewHolder(object content, DataTemplate template, Context context)
		{
			if (template != null)
			{
				var itemContentView = new ItemContentView(context);
				return new TemplatedItemViewHolder(itemContentView, template, isSelectionEnabled: false);
			}

			if (content is View formsView)
			{
				var viewHolder = SimpleViewHolder.FromFormsView(formsView, context);

				// Propagate the binding context, visual, etc. from the ItemsView to the header/footer
				ItemsView.AddLogicalChild(viewHolder.View);

				return viewHolder;
			}

			// No template, Footer is not a Forms View, so just display Footer.ToString
			return SimpleViewHolder.FromText(content?.ToString(), context, false);
		}

		protected RecyclerView.ViewHolder CreateEmptyViewHolder(object content, DataTemplate template, ViewGroup parent)
		{
			var context = parent.Context;

			if (template == null)
			{
				if (!(content is View formsView))
				{
					// No template, EmptyView is not a Forms View, so just display EmptyView.ToString
					return SimpleViewHolder.FromText(content?.ToString(), context);
				}

				// EmptyView is a Forms View; display that
				return SimpleViewHolder.FromFormsView(formsView, context, () => GetWidth(parent), () => GetHeight(parent), ItemsView);
			}

			var itemContentView = new SizedItemContentView(parent.Context, () => GetWidth(parent), () => GetHeight(parent));
			return new TemplatedItemViewHolder(itemContentView, template, isSelectionEnabled: false);
		}

		protected void BindTemplatedItemViewHolder(TemplatedItemViewHolder templatedItemViewHolder, object context)
		{
			templatedItemViewHolder.Bind(context, ItemsView);
		}

		bool IsHeader(int position)
		{
			if (Header == null && HeaderTemplate == null)
				return false;

			return position == 0;
		}

		bool IsFooter(int position)
		{
			if (Footer == null && FooterTemplate == null)
				return false;

			return position == ItemCount - 1;
		}

		bool IsEmpty(int position)
		{
			if (EmptyView == null && EmptyViewTemplate == null)
				return false;

			return (Header == null && HeaderTemplate == null) ? position == 0 : position == 1;
		}

		int GetHeight(ViewGroup parent)
		{
			var headerFooterHeight = parent.Context.ToPixels(_headerHeight + _footerHeight);
			return Math.Abs((int)(parent.MeasuredHeight - headerFooterHeight));
		}

		int GetWidth(ViewGroup parent)
		{
			return parent.MeasuredWidth;
		}

		void UpdateHeaderFooterHeight(object item, bool isHeader)
		{
			if (item == null)
				return;

			var sizeRequest = new SizeRequest(new Size(0, 0));

			if (item is View view)
				sizeRequest = view.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);

			if (item is DataTemplate dataTemplate)
			{
				var content = dataTemplate.CreateContent() as View;
				sizeRequest = content.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
			}

			var itemHeight = (int)sizeRequest.Request.Height;

			if (isHeader)
				_headerHeight = itemHeight;
			else
				_footerHeight = itemHeight;
		}
	}
}