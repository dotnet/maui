using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;
using LP = Android.Views.ViewGroup.LayoutParams;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellFlyoutRecyclerAdapter : RecyclerView.Adapter
	{
		IShellContext _shellContext;
		List<AdapterListItem> _listItems;
		List<List<Element>> _flyoutGroupings;
		Action<Element> _selectedCallback;
		bool _disposed;
		IMauiContext MauiContext => _shellContext.Shell.Handler.MauiContext;

		public ShellFlyoutRecyclerAdapter(IShellContext shellContext, Action<Element> selectedCallback)
		{
			HasStableIds = true;
			_shellContext = shellContext;

			ShellController.FlyoutItemsChanged += OnFlyoutItemsChanged;

			_listItems = GenerateItemList();
			_selectedCallback = selectedCallback;
		}

		public override int ItemCount => _listItems.Count;

		protected Shell Shell => _shellContext.Shell;

		IShellController ShellController => (IShellController)Shell;

		protected virtual DataTemplate DefaultItemTemplate => null;

		protected virtual DataTemplate DefaultMenuItemTemplate => null;

		public override int GetItemViewType(int position)
		{
			return _listItems[position].Index;
		}

		DataTemplate GetDataTemplate(int viewTypeId)
		{
			AdapterListItem item = null;

			foreach (var ali in _listItems)
			{
				if (viewTypeId == ali.Index)
				{
					item = ali;
					break;
				}
			}

			DataTemplate dataTemplate = ShellController.GetFlyoutItemDataTemplate(item.Element);
			if (item.Element is IMenuItemController)
			{
				if (DefaultMenuItemTemplate != null && Shell.MenuItemTemplate == dataTemplate)
					dataTemplate = DefaultMenuItemTemplate;
			}
			else
			{
				if (DefaultItemTemplate != null && Shell.ItemTemplate == dataTemplate)
					dataTemplate = DefaultItemTemplate;
			}

			var template = dataTemplate.SelectDataTemplate(item.Element, Shell);
			return template;
		}

		public override void OnViewRecycled(Java.Lang.Object holder)
		{
			if (holder is ElementViewHolder evh)
			{
				// only clear out the Element if the item has been removed
				bool found = false;
				foreach (var item in _listItems)
				{
					if (item.Element == evh.Element)
					{
						found = true;
						break;
					}
				}

				if (!found)
					evh.Element = null;
			}

			base.OnViewRecycled(holder);
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			var item = _listItems[position];
			var elementHolder = (ElementViewHolder)holder;

			elementHolder.Bar.Visibility = item.DrawTopLine ? ViewStates.Visible : ViewStates.Gone;
			elementHolder.Element = item.Element;
		}

		class ShellLinearLayout : LinearLayout
		{
			public ShellLinearLayout(global::Android.Content.Context context) : base(context)
			{
			}

			internal View Content { get; set; }
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var template = GetDataTemplate(viewType);

			var content = (View)template.CreateContent();

			var linearLayout = new ShellLinearLayout(parent.Context)
			{
				Orientation = Orientation.Vertical,
				LayoutParameters = new RecyclerView.LayoutParams(LP.MatchParent, LP.WrapContent),
				Content = content
			};

			var bar = new AView(parent.Context);
			bar.SetBackgroundColor(Colors.Black.MultiplyAlpha(0.14f).ToPlatform());
			bar.LayoutParameters = new LP(LP.MatchParent, (int)parent.Context.ToPixels(1));
			linearLayout.AddView(bar);

			var container = new ContainerView(parent.Context, content, MauiContext);
			container.MatchWidth = true;
			container.LayoutParameters = new LP(LP.MatchParent, LP.WrapContent);
			linearLayout.AddView(container);

			return new ElementViewHolder(content, linearLayout, bar, _selectedCallback, _shellContext.Shell);
		}

		protected virtual List<AdapterListItem> GenerateItemList()
		{
			var result = new List<AdapterListItem>();
			_listItems = _listItems ?? result;

			List<List<Element>> grouping = ((IShellController)_shellContext.Shell).GenerateFlyoutGrouping();

			if (_flyoutGroupings == grouping)
				return _listItems;

			_flyoutGroupings = grouping;

			bool skip = true;

			foreach (var sublist in grouping)
			{
				bool first = !skip;
				foreach (var element in sublist)
				{
					AdapterListItem toAdd = null;
					foreach (var existingItem in _listItems)
					{
						if (existingItem.Element == element)
						{
							existingItem.DrawTopLine = first;
							toAdd = existingItem;
						}
					}

					toAdd = toAdd ?? new AdapterListItem(element, first);
					result.Add(toAdd);
					first = false;
				}
				skip = false;
			}

			return result;
		}

		protected virtual void OnFlyoutItemsChanged(object sender, EventArgs e)
		{
			var newListItems = GenerateItemList();

			if (newListItems != _listItems)
			{
				_listItems = newListItems;
				NotifyDataSetChanged();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				Disconnect();
			}

			base.Dispose(disposing);
		}

		internal void Disconnect()
		{
			if (Shell is IShellController scc)
				scc.FlyoutItemsChanged -= OnFlyoutItemsChanged;

			_listItems = null;
			_selectedCallback = null;
			_shellContext = null;
		}

		public class AdapterListItem
		{
			// This ensures that we have a stable id for each element
			// if the elements change position
			static int IndexCounter = 0;

			public AdapterListItem(Element element, bool drawTopLine = false)
			{
				DrawTopLine = drawTopLine;
				Element = element;
				Index = IndexCounter++;
			}

			public int Index { get; }
			public bool DrawTopLine { get; set; }
			public Element Element { get; set; }
		}

		public class ElementViewHolder : RecyclerView.ViewHolder
		{
			Action<Element> _selectedCallback;
			Element _element;
			AView _itemView;
			bool _disposed;
			Shell _shell;

			public ElementViewHolder(View view, AView itemView, AView bar, Action<Element> selectedCallback, Shell shell) : base(itemView)
			{
				_itemView = itemView;
				itemView.Click += OnClicked;
				View = view;
				Bar = bar;
				_selectedCallback = selectedCallback;
				_shell = shell;
			}

			public View View { get; }
			public AView Bar { get; }
			public Element Element
			{
				get { return _element; }
				set
				{
					if (_element == value)
						return;

					if (View.Parent is BaseShellItem bsi)
						bsi.RemoveLogicalChild(View);
					else
						_shell.RemoveLogicalChild(View);

					if (_element != null && _element is BaseShellItem)
					{
						_element.PropertyChanged -= OnElementPropertyChanged;
					}

					_element = value;

					// Set binding context before calling AddLogicalChild so parent binding context doesn't propagate to view
					View.BindingContext = value;

					if (_element != null)
					{
						if (value is BaseShellItem bsiNew)
							bsiNew.AddLogicalChild(View);
						else
							_shell.AddLogicalChild(View);

						_element.PropertyChanged += OnElementPropertyChanged;
						UpdateVisualState();
					}
				}
			}

			void UpdateVisualState()
			{
				if (Element is BaseShellItem baseShellItem && baseShellItem != null)
				{
					if (baseShellItem.IsChecked)
						VisualStateManager.GoToState(View, "Selected");
					else
						VisualStateManager.GoToState(View, "Normal");
				}
			}

			void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				if (e.PropertyName == BaseShellItem.IsCheckedProperty.PropertyName)
					UpdateVisualState();
			}

			void OnClicked(object sender, EventArgs e)
			{
				if (Element == null)
					return;

				_selectedCallback(Element);
			}

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				_disposed = true;

				if (disposing)
				{
					_itemView.Click -= OnClicked;

					Element = null;
					_itemView = null;
					_selectedCallback = null;
				}

				base.Dispose(disposing);
			}
		}
	}
}
