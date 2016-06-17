using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms.Platform;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_ListViewRenderer))]
	public class ListView : ItemsView<Cell>, IListViewController

	{
		public static readonly BindableProperty IsPullToRefreshEnabledProperty = BindableProperty.Create("IsPullToRefreshEnabled", typeof(bool), typeof(ListView), false);

		public static readonly BindableProperty IsRefreshingProperty = BindableProperty.Create("IsRefreshing", typeof(bool), typeof(ListView), false, BindingMode.TwoWay);

		public static readonly BindableProperty RefreshCommandProperty = BindableProperty.Create("RefreshCommand", typeof(ICommand), typeof(ListView), null, propertyChanged: OnRefreshCommandChanged);

		public static readonly BindableProperty HeaderProperty = BindableProperty.Create("Header", typeof(object), typeof(ListView), null, propertyChanged: OnHeaderChanged);

		public static readonly BindableProperty HeaderTemplateProperty = BindableProperty.Create("HeaderTemplate", typeof(DataTemplate), typeof(ListView), null, propertyChanged: OnHeaderTemplateChanged,
			validateValue: ValidateHeaderFooterTemplate);

		public static readonly BindableProperty FooterProperty = BindableProperty.Create("Footer", typeof(object), typeof(ListView), null, propertyChanged: OnFooterChanged);

		public static readonly BindableProperty FooterTemplateProperty = BindableProperty.Create("FooterTemplate", typeof(DataTemplate), typeof(ListView), null, propertyChanged: OnFooterTemplateChanged,
			validateValue: ValidateHeaderFooterTemplate);

		public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create("SelectedItem", typeof(object), typeof(ListView), null, BindingMode.OneWayToSource,
			propertyChanged: OnSelectedItemChanged);

		public static readonly BindableProperty HasUnevenRowsProperty = BindableProperty.Create("HasUnevenRows", typeof(bool), typeof(ListView), false);

		public static readonly BindableProperty RowHeightProperty = BindableProperty.Create("RowHeight", typeof(int), typeof(ListView), -1);

		public static readonly BindableProperty GroupHeaderTemplateProperty = BindableProperty.Create("GroupHeaderTemplate", typeof(DataTemplate), typeof(ListView), null,
			propertyChanged: OnGroupHeaderTemplateChanged);

		public static readonly BindableProperty IsGroupingEnabledProperty = BindableProperty.Create("IsGroupingEnabled", typeof(bool), typeof(ListView), false);

		public static readonly BindableProperty SeparatorVisibilityProperty = BindableProperty.Create("SeparatorVisibility", typeof(SeparatorVisibility), typeof(ListView), SeparatorVisibility.Default);

		public static readonly BindableProperty SeparatorColorProperty = BindableProperty.Create("SeparatorColor", typeof(Color), typeof(ListView), Color.Default);

		BindingBase _groupDisplayBinding;

		BindingBase _groupShortNameBinding;
		Element _headerElement;
		Element _footerElement;

		ScrollToRequestedEventArgs _pendingScroll;
		int _previousGroupSelected = -1;
		int _previousRowSelected = -1;

		/// <summary>
		///     Controls whether anything happens in BeginRefresh(), is set based on RefreshCommand.CanExecute
		/// </summary>
		bool _refreshAllowed = true;

		public ListView()
		{
			VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;

			TemplatedItems.IsGroupingEnabledProperty = IsGroupingEnabledProperty;
			TemplatedItems.GroupHeaderTemplateProperty = GroupHeaderTemplateProperty;
		}

		public ListView([Parameter("CachingStrategy")] ListViewCachingStrategy cachingStrategy) : this()
		{
			if (Device.OS == TargetPlatform.Android || Device.OS == TargetPlatform.iOS)
				CachingStrategy = cachingStrategy;
		}

		public object Footer
		{
			get { return GetValue(FooterProperty); }
			set { SetValue(FooterProperty, value); }
		}

		public DataTemplate FooterTemplate
		{
			get { return (DataTemplate)GetValue(FooterTemplateProperty); }
			set { SetValue(FooterTemplateProperty, value); }
		}

		public BindingBase GroupDisplayBinding
		{
			get { return _groupDisplayBinding; }
			set
			{
				if (_groupDisplayBinding == value)
					return;

				OnPropertyChanging();
				BindingBase oldValue = value;
				_groupDisplayBinding = value;
				OnGroupDisplayBindingChanged(this, oldValue, _groupDisplayBinding);
				TemplatedItems.GroupDisplayBinding = value;
				OnPropertyChanged();
			}
		}

		public DataTemplate GroupHeaderTemplate
		{
			get { return (DataTemplate)GetValue(GroupHeaderTemplateProperty); }
			set { SetValue(GroupHeaderTemplateProperty, value); }
		}

		public BindingBase GroupShortNameBinding
		{
			get { return _groupShortNameBinding; }
			set
			{
				if (_groupShortNameBinding == value)
					return;

				OnPropertyChanging();
				_groupShortNameBinding = value;
				TemplatedItems.GroupShortNameBinding = value;
				OnPropertyChanged();
			}
		}

		public bool HasUnevenRows
		{
			get { return (bool)GetValue(HasUnevenRowsProperty); }
			set { SetValue(HasUnevenRowsProperty, value); }
		}

		public object Header
		{
			get { return GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		public DataTemplate HeaderTemplate
		{
			get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
			set { SetValue(HeaderTemplateProperty, value); }
		}

		public bool IsGroupingEnabled
		{
			get { return (bool)GetValue(IsGroupingEnabledProperty); }
			set { SetValue(IsGroupingEnabledProperty, value); }
		}

		public bool IsPullToRefreshEnabled
		{
			get { return (bool)GetValue(IsPullToRefreshEnabledProperty); }
			set { SetValue(IsPullToRefreshEnabledProperty, value); }
		}

		public bool IsRefreshing
		{
			get { return (bool)GetValue(IsRefreshingProperty); }
			set { SetValue(IsRefreshingProperty, value); }
		}

		public ICommand RefreshCommand
		{
			get { return (ICommand)GetValue(RefreshCommandProperty); }
			set { SetValue(RefreshCommandProperty, value); }
		}

		public int RowHeight
		{
			get { return (int)GetValue(RowHeightProperty); }
			set { SetValue(RowHeightProperty, value); }
		}

		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public Color SeparatorColor
		{
			get { return (Color)GetValue(SeparatorColorProperty); }
			set { SetValue(SeparatorColorProperty, value); }
		}

		public SeparatorVisibility SeparatorVisibility
		{
			get { return (SeparatorVisibility)GetValue(SeparatorVisibilityProperty); }
			set { SetValue(SeparatorVisibilityProperty, value); }
		}

		internal ListViewCachingStrategy CachingStrategy { get; private set; }
		ListViewCachingStrategy IListViewController.CachingStrategy
		{
			get
			{
				return CachingStrategy;
			}
		}

		bool RefreshAllowed
		{
			set
			{
				if (_refreshAllowed == value)
					return;

				_refreshAllowed = value;
				OnPropertyChanged();
			}
			get { return _refreshAllowed; }
		}

		Element IListViewController.FooterElement
		{
			get { return _footerElement; }
		}

		Element IListViewController.HeaderElement
		{
			get { return _headerElement; }
		}

		bool IListViewController.RefreshAllowed
		{
			get { return RefreshAllowed; }
		}

		void IListViewController.SendCellAppearing(Cell cell)
		{
			EventHandler<ItemVisibilityEventArgs> handler = ItemAppearing;
			if (handler != null)
				handler(this, new ItemVisibilityEventArgs(cell.BindingContext));
		}

		void IListViewController.SendCellDisappearing(Cell cell)
		{
			EventHandler<ItemVisibilityEventArgs> handler = ItemDisappearing;
			if (handler != null)
				handler(this, new ItemVisibilityEventArgs(cell.BindingContext));
		}

		void IListViewController.SendRefreshing()
		{
			BeginRefresh();
		}

		public void BeginRefresh()
		{
			if (!RefreshAllowed)
				return;

			SetValueCore(IsRefreshingProperty, true);
			OnRefreshing(EventArgs.Empty);

			ICommand command = RefreshCommand;
			if (command != null)
				command.Execute(null);
		}

		public void EndRefresh()
		{
			SetValueCore(IsRefreshingProperty, false);
		}

		public event EventHandler<ItemVisibilityEventArgs> ItemAppearing;

		public event EventHandler<ItemVisibilityEventArgs> ItemDisappearing;

		public event EventHandler<SelectedItemChangedEventArgs> ItemSelected;

		public event EventHandler<ItemTappedEventArgs> ItemTapped;

		public event EventHandler Refreshing;

		public void ScrollTo(object item, ScrollToPosition position, bool animated)
		{
			if (!Enum.IsDefined(typeof(ScrollToPosition), position))
				throw new ArgumentException("position is not a valid ScrollToPosition", "position");

			var args = new ScrollToRequestedEventArgs(item, position, animated);
			if (IsPlatformEnabled)
				OnScrollToRequested(args);
			else
				_pendingScroll = args;
		}

		public void ScrollTo(object item, object group, ScrollToPosition position, bool animated)
		{
			if (!IsGroupingEnabled)
				throw new InvalidOperationException("Grouping is not enabled");
			if (!Enum.IsDefined(typeof(ScrollToPosition), position))
				throw new ArgumentException("position is not a valid ScrollToPosition", "position");

			var args = new ScrollToRequestedEventArgs(item, group, position, animated);
			if (IsPlatformEnabled)
				OnScrollToRequested(args);
			else
				_pendingScroll = args;
		}

		protected override Cell CreateDefault(object item)
		{
			string text = null;
			if (item != null)
				text = item.ToString();

			return new TextCell { Text = text };
		}

		[Obsolete("Use OnMeasure")]
		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			var minimumSize = new Size(40, 40);
			Size request;

			double width = Math.Min(Device.Info.ScaledScreenSize.Width, Device.Info.ScaledScreenSize.Height);

			var list = ItemsSource as IList;
			if (list != null && HasUnevenRows == false && RowHeight > 0 && !IsGroupingEnabled)
			{
				// we can calculate this
				request = new Size(width, list.Count * RowHeight);
			}
			else
			{
				// probably not worth it
				request = new Size(width, Math.Max(Device.Info.ScaledScreenSize.Width, Device.Info.ScaledScreenSize.Height));
			}

			return new SizeRequest(request, minimumSize);
		}

		protected override void SetupContent(Cell content, int index)
		{
			base.SetupContent(content, index);
			var viewCell = content as ViewCell;
			if (viewCell != null && viewCell.View != null && HasUnevenRows)
				viewCell.View.ComputedConstraint = LayoutConstraint.None;
			content.Parent = this;

		}

		protected override void UnhookContent(Cell content)
		{
			base.UnhookContent(content);
			content.Parent = null;
		}

		Cell IListViewController.CreateDefaultCell(object item)
		{
			return CreateDefault(item);
		}

		string IListViewController.GetDisplayTextFromGroup(object cell)
		{
			int groupIndex = TemplatedItems.GetGlobalIndexOfGroup(cell);
			var group = TemplatedItems.GetGroup(groupIndex);

			string displayBinding = null;

			if (GroupDisplayBinding != null)
				displayBinding = group.Name;

			if (GroupShortNameBinding != null)
				displayBinding = group.ShortName;

			// TODO: what if they set both? should it default to the ShortName, like it will here?
			// ShortNames binding did not appear to be functional before.
			return displayBinding;
		}

		internal void NotifyRowTapped(int groupIndex, int inGroupIndex, Cell cell = null)
		{
			var group = TemplatedItems.GetGroup(groupIndex);

			bool changed = _previousGroupSelected != groupIndex || _previousRowSelected != inGroupIndex;

			_previousRowSelected = inGroupIndex;
			_previousGroupSelected = groupIndex;
			if (cell == null)
			{
				cell = group[inGroupIndex];
			}

			// Set SelectedItem before any events so we don't override any changes they may have made.
			SetValueCore(SelectedItemProperty, cell.BindingContext, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearDynamicResource | (changed ? SetValueFlags.RaiseOnEqual : 0));

			cell.OnTapped();

			ItemTapped?.Invoke(this, new ItemTappedEventArgs(group, cell.BindingContext));
		}

		internal void NotifyRowTapped(int index, Cell cell = null)
		{
			if (IsGroupingEnabled)
			{
				int leftOver;
				int groupIndex = TemplatedItems.GetGroupIndexFromGlobal(index, out leftOver);

				NotifyRowTapped(groupIndex, leftOver - 1, cell);
			}
			else
				NotifyRowTapped(0, index, cell);
		}

		void IListViewController.NotifyRowTapped(int index, Cell cell)
		{
			NotifyRowTapped(index, cell);
		}

		void IListViewController.NotifyRowTapped(int index, int inGroupIndex, Cell cell)
		{
			NotifyRowTapped(index, inGroupIndex, cell);
		}

		internal override void OnIsPlatformEnabledChanged()
		{
			base.OnIsPlatformEnabledChanged();

			if (IsPlatformEnabled && _pendingScroll != null)
			{
				OnScrollToRequested(_pendingScroll);
				_pendingScroll = null;
			}
		}

		internal event EventHandler<ScrollToRequestedEventArgs> ScrollToRequested;
		event EventHandler<ScrollToRequestedEventArgs> IListViewController.ScrollToRequested { add { ScrollToRequested += value; } remove { ScrollToRequested -= value; } }

		void OnCommandCanExecuteChanged(object sender, EventArgs eventArgs)
		{
			RefreshAllowed = RefreshCommand.CanExecute(null);
		}

		static void OnFooterChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var lv = (ListView)bindable;
			lv.OnHeaderOrFooterChanged(ref lv._footerElement, "FooterElement", newValue, lv.FooterTemplate, false);
		}

		static void OnFooterTemplateChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var lv = (ListView)bindable;
			lv.OnHeaderOrFooterChanged(ref lv._footerElement, "FooterElement", lv.Footer, (DataTemplate)newValue, true);
		}

		static void OnGroupDisplayBindingChanged(BindableObject bindable, BindingBase oldValue, BindingBase newValue)
		{
			var lv = (ListView)bindable;
			if (newValue != null && lv.GroupHeaderTemplate != null)
			{
				lv.GroupHeaderTemplate = null;
				Log.Warning("ListView", "GroupHeaderTemplate and GroupDisplayBinding can not be set at the same time, setting GroupHeaderTemplate to null");
			}
		}

		static void OnGroupHeaderTemplateChanged(BindableObject bindable, object oldvalue, object newValue)
		{
			var lv = (ListView)bindable;
			if (newValue != null && lv.GroupDisplayBinding != null)
			{
				lv.GroupDisplayBinding = null;
				Debug.WriteLine("GroupHeaderTemplate and GroupDisplayBinding can not be set at the same time, setting GroupDisplayBinding to null");
			}
		}

		static void OnHeaderChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var lv = (ListView)bindable;
			lv.OnHeaderOrFooterChanged(ref lv._headerElement, "HeaderElement", newValue, lv.HeaderTemplate, false);
		}

		void OnHeaderOrFooterChanged(ref Element storage, string property, object dataObject, DataTemplate template, bool templateChanged)
		{
			if (dataObject == null)
			{
				if (!templateChanged)
				{
					OnPropertyChanging(property);
					storage = null;
					OnPropertyChanged(property);
				}

				return;
			}

			if (template == null)
			{
				var view = dataObject as Element;
				if (view == null || view is Page)
					view = new Label { Text = dataObject.ToString() };

				view.Parent = this;
				OnPropertyChanging(property);
				storage = view;
				OnPropertyChanged(property);
			}
			else if (storage == null || templateChanged)
			{
				OnPropertyChanging(property);
				storage = template.CreateContent() as Element;
				if (storage != null)
				{
					storage.BindingContext = dataObject;
					storage.Parent = this;
				}
				OnPropertyChanged(property);
			}
			else
			{
				storage.BindingContext = dataObject;
			}
		}

		static void OnHeaderTemplateChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var lv = (ListView)bindable;
			lv.OnHeaderOrFooterChanged(ref lv._headerElement, "HeaderElement", lv.Header, (DataTemplate)newValue, true);
		}

		static void OnRefreshCommandChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var lv = (ListView)bindable;
			var oldCommand = (ICommand)oldValue;
			var command = (ICommand)newValue;

			lv.OnRefreshCommandChanged(oldCommand, command);
		}

		void OnRefreshCommandChanged(ICommand oldCommand, ICommand newCommand)
		{
			if (oldCommand != null)
			{
				oldCommand.CanExecuteChanged -= OnCommandCanExecuteChanged;
			}

			if (newCommand != null)
			{
				newCommand.CanExecuteChanged += OnCommandCanExecuteChanged;
				RefreshAllowed = newCommand.CanExecute(null);
			}
			else
			{
				RefreshAllowed = true;
			}
		}

		void OnRefreshing(EventArgs e)
		{
			EventHandler handler = Refreshing;
			if (handler != null)
				handler(this, e);
		}

		void OnScrollToRequested(ScrollToRequestedEventArgs e)
		{
			EventHandler<ScrollToRequestedEventArgs> handler = ScrollToRequested;
			if (handler != null)
				handler(this, e);
		}

		static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var list = (ListView)bindable;
			if (list.ItemSelected != null)
				list.ItemSelected(list, new SelectedItemChangedEventArgs(newValue));
		}

		static bool ValidateHeaderFooterTemplate(BindableObject bindable, object value)
		{
			if (value == null)
				return true;
			var template = (DataTemplate)value;
			return template.CreateContent() is View;
		}
	}
}