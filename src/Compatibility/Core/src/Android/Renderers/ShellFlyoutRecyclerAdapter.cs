using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls.Internals;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public class ShellFlyoutRecyclerAdapter : RecyclerView.Adapter
	{
		readonly IShellContext _shellContext;
		List<AdapterListItem> _listItems;
		List<List<Element>> _flyoutGroupings;
		Action<Element> _selectedCallback;
		bool _disposed;

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

		class LinearLayoutWithFocus : LinearLayout, ITabStop, IVisualElementRenderer
		{
			public LinearLayoutWithFocus(global::Android.Content.Context context) : base(context)
			{
			}

			AView ITabStop.TabStop => this;

			#region IVisualElementRenderer

			VisualElement IVisualElementRenderer.Element => Content?.BindingContext as VisualElement;

			VisualElementTracker IVisualElementRenderer.Tracker => null;

			ViewGroup IVisualElementRenderer.ViewGroup => this;

			AView IVisualElementRenderer.View => this;

			SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint) => new SizeRequest(new Size(100, 100));

			void IVisualElementRenderer.SetElement(VisualElement element) { }

			void IVisualElementRenderer.SetLabelFor(int? id) { }

			void IVisualElementRenderer.UpdateLayout() { }

#pragma warning disable 67
			public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
			public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;
#pragma warning restore 67

			#endregion IVisualElementRenderer

			internal View Content { get; set; }

			public override AView FocusSearch([GeneratedEnum] FocusSearchDirection direction)
			{
				var element = Content?.BindingContext as ITabStopElement;
				if (element == null)
					return base.FocusSearch(direction);

				int maxAttempts = 0;
				var tabIndexes = element?.GetTabIndexesOnParentPage(out maxAttempts);
				if (tabIndexes == null)
					return base.FocusSearch(direction);

				// use OS default--there's no need for us to keep going if there's one or fewer tab indexes!
				if (tabIndexes.Count <= 1)
					return base.FocusSearch(direction);

				int tabIndex = element.TabIndex;
				AView control = null;
				int attempt = 0;
				bool forwardDirection = !(
					(direction & FocusSearchDirection.Backward) != 0 ||
					(direction & FocusSearchDirection.Left) != 0 ||
					(direction & FocusSearchDirection.Up) != 0);

				do
				{
					element = element.FindNextElement(forwardDirection, tabIndexes, ref tabIndex);
					var renderer = (element as BindableObject).GetValue(AppCompat.Platform.RendererProperty);
					control = (renderer as ITabStop)?.TabStop;
				} while (!(control?.Focusable == true || ++attempt >= maxAttempts));

				return control?.Focusable == true ? control : null;
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var template = GetDataTemplate(viewType);

			var content = (View)template.CreateContent();

			var linearLayout = new LinearLayoutWithFocus(parent.Context)
			{
				Orientation = Orientation.Vertical,
				LayoutParameters = new RecyclerView.LayoutParams(LP.MatchParent, LP.WrapContent),
				Content = content
			};

			var bar = new AView(parent.Context);
			bar.SetBackgroundColor(Color.Black.MultiplyAlpha(0.14).ToAndroid());
			bar.LayoutParameters = new LP(LP.MatchParent, (int)parent.Context.ToPixels(1));
			linearLayout.AddView(bar);

			var container = new ContainerView(parent.Context, content);
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
				((IShellController)Shell).FlyoutItemsChanged -= OnFlyoutItemsChanged;

				_listItems = null;
				_selectedCallback = null;
			}

			base.Dispose(disposing);
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

			[Obsolete]
			public ElementViewHolder(View view, AView itemView, AView bar, Action<Element> selectedCallback) : this(view, itemView, bar, selectedCallback, null)
			{
				_itemView = itemView;
				itemView.Click += OnClicked;
				View = view;
				Bar = bar;
				_selectedCallback = selectedCallback;
			}

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

					_shell.RemoveLogicalChild(View);
					if (_element != null && _element is BaseShellItem)
					{
						_element.ClearValue(AppCompat.Platform.RendererProperty);
						_element.PropertyChanged -= OnElementPropertyChanged;
					}

					_element = value;

					// Set binding context before calling AddLogicalChild so parent binding context doesn't propagate to view
					View.BindingContext = value;

					if (_element != null)
					{
						_shell.AddLogicalChild(View);
						FastRenderers.AutomationPropertiesProvider.AccessibilitySettingsChanged(_itemView, value);
						_element.SetValue(AppCompat.Platform.RendererProperty, _itemView);
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