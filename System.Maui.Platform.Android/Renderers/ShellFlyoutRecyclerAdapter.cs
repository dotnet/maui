using Android.Runtime;
#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Maui.Internals;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace System.Maui.Platform.Android
{
	public class ShellFlyoutRecyclerAdapter : RecyclerView.Adapter
	{
		readonly IShellContext _shellContext;
		List<AdapterListItem> _listItems;
		Dictionary<int, DataTemplate> _templateMap = new Dictionary<int, DataTemplate>();
		Action<Element> _selectedCallback;
		bool _disposed;
		ElementViewHolder _elementViewHolder;

		public ShellFlyoutRecyclerAdapter(IShellContext shellContext, Action<Element> selectedCallback)
		{
			_shellContext = shellContext;

			ShellController.StructureChanged += OnShellStructureChanged;

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
			var item = _listItems[position];
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
			var id = ((IDataTemplateController)template).Id;

			_templateMap[id] = template;

			return id;
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
					var renderer = (element as BindableObject).GetValue(Platform.RendererProperty);
					control = (renderer as ITabStop)?.TabStop;
				} while (!(control?.Focusable == true || ++attempt >= maxAttempts));

				return control?.Focusable == true ? control : null;
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var template = _templateMap[viewType];

			var content = (View)template.CreateContent();
			content.Parent = _shellContext.Shell;

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

			_elementViewHolder = new ElementViewHolder(content, linearLayout, bar, _selectedCallback);

			return _elementViewHolder;
		}

		protected virtual List<AdapterListItem> GenerateItemList()
		{
			var result = new List<AdapterListItem>();

			var grouping = ((IShellController)_shellContext.Shell).GenerateFlyoutGrouping();

			bool skip = true;

			foreach (var sublist in grouping)
			{
				bool first = !skip;
				foreach (var element in sublist)
				{
					result.Add(new AdapterListItem(element, first));
					first = false;
				}
				skip = false;
			}

			return result;
		}

		protected virtual void OnShellStructureChanged(object sender, EventArgs e)
		{
			_listItems = GenerateItemList();
			NotifyDataSetChanged();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				((IShellController)Shell).StructureChanged -= OnShellStructureChanged;

				_elementViewHolder?.Dispose();

				_listItems = null;
				_selectedCallback = null;
				_elementViewHolder = null;
			}

			base.Dispose(disposing);
		}

		public class AdapterListItem
		{
			public AdapterListItem(Element element, bool drawTopLine = false)
			{
				DrawTopLine = drawTopLine;
				Element = element;
			}

			public bool DrawTopLine { get; set; }
			public Element Element { get; set; }
		}

		public class ElementViewHolder : RecyclerView.ViewHolder
		{
			Action<Element> _selectedCallback;
			Element _element;
			AView _itemView;
			bool _disposed;

			public ElementViewHolder(View view, AView itemView, AView bar, Action<Element> selectedCallback) : base(itemView)
			{
				_itemView = itemView;
				itemView.Click += OnClicked;
				View = view;
				Bar = bar;
				_selectedCallback = selectedCallback;
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

					if (_element != null && _element is BaseShellItem)
					{
						_element.ClearValue(Platform.RendererProperty);
						_element.PropertyChanged -= OnElementPropertyChanged;
					}

					_element = value;
					View.BindingContext = value;

					if (_element != null)
					{
						_element.SetValue(Platform.RendererProperty, _itemView);
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