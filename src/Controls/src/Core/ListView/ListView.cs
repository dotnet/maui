#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="Type[@FullName='Microsoft.Maui.Controls.ListView']/Docs/*" />
	public class ListView : ItemsView<Cell>, IListViewController, IElementConfiguration<ListView>, IVisualTreeElement
	{
		// The ListViewRenderer has some odd behavior with LogicalChildren
		// https://github.com/xamarin/Xamarin.Forms/pull/12057
		// Ideally we'd fix the ListViewRenderer so we don't have this separation
		readonly List<Element> _visualChildren = new List<Element>();
		IReadOnlyList<IVisualTreeElement> IVisualTreeElement.GetVisualChildren() => _visualChildren;

		/// <summary>Bindable property for <see cref="IsPullToRefreshEnabled"/>.</summary>
		public static readonly BindableProperty IsPullToRefreshEnabledProperty = BindableProperty.Create("IsPullToRefreshEnabled", typeof(bool), typeof(ListView), false);

		/// <summary>Bindable property for <see cref="IsRefreshing"/>.</summary>
		public static readonly BindableProperty IsRefreshingProperty = BindableProperty.Create("IsRefreshing", typeof(bool), typeof(ListView), false, BindingMode.TwoWay);

		/// <summary>Bindable property for <see cref="RefreshCommand"/>.</summary>
		public static readonly BindableProperty RefreshCommandProperty = BindableProperty.Create("RefreshCommand", typeof(ICommand), typeof(ListView), null, propertyChanged: OnRefreshCommandChanged);

		/// <summary>Bindable property for <see cref="Header"/>.</summary>
		public static readonly BindableProperty HeaderProperty = BindableProperty.Create("Header", typeof(object), typeof(ListView), null, propertyChanged: OnHeaderChanged);

		/// <summary>Bindable property for <see cref="HeaderTemplate"/>.</summary>
		public static readonly BindableProperty HeaderTemplateProperty = BindableProperty.Create("HeaderTemplate", typeof(DataTemplate), typeof(ListView), null, propertyChanged: OnHeaderTemplateChanged,
			validateValue: ValidateHeaderFooterTemplate);

		/// <summary>Bindable property for <see cref="Footer"/>.</summary>
		public static readonly BindableProperty FooterProperty = BindableProperty.Create("Footer", typeof(object), typeof(ListView), null, propertyChanged: OnFooterChanged);

		/// <summary>Bindable property for <see cref="FooterTemplate"/>.</summary>
		public static readonly BindableProperty FooterTemplateProperty = BindableProperty.Create("FooterTemplate", typeof(DataTemplate), typeof(ListView), null, propertyChanged: OnFooterTemplateChanged,
			validateValue: ValidateHeaderFooterTemplate);

		/// <summary>Bindable property for <see cref="SelectedItem"/>.</summary>
		public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create("SelectedItem", typeof(object), typeof(ListView), null, BindingMode.OneWayToSource,
			propertyChanged: OnSelectedItemChanged);

		/// <summary>Bindable property for <see cref="SelectionMode"/>.</summary>
		public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(nameof(SelectionMode), typeof(ListViewSelectionMode), typeof(ListView), ListViewSelectionMode.Single);

		/// <summary>Bindable property for <see cref="HasUnevenRows"/>.</summary>
		public static readonly BindableProperty HasUnevenRowsProperty = BindableProperty.Create("HasUnevenRows", typeof(bool), typeof(ListView), false);

		/// <summary>Bindable property for <see cref="RowHeight"/>.</summary>
		public static readonly BindableProperty RowHeightProperty = BindableProperty.Create("RowHeight", typeof(int), typeof(ListView), -1);

		/// <summary>Bindable property for <see cref="GroupHeaderTemplate"/>.</summary>
		public static readonly BindableProperty GroupHeaderTemplateProperty = BindableProperty.Create("GroupHeaderTemplate", typeof(DataTemplate), typeof(ListView), null,
			propertyChanged: OnGroupHeaderTemplateChanged);

		/// <summary>Bindable property for <see cref="IsGroupingEnabled"/>.</summary>
		public static readonly BindableProperty IsGroupingEnabledProperty = BindableProperty.Create("IsGroupingEnabled", typeof(bool), typeof(ListView), false);

		/// <summary>Bindable property for <see cref="SeparatorVisibility"/>.</summary>
		public static readonly BindableProperty SeparatorVisibilityProperty = BindableProperty.Create("SeparatorVisibility", typeof(SeparatorVisibility), typeof(ListView), SeparatorVisibility.Default);

		/// <summary>Bindable property for <see cref="SeparatorColor"/>.</summary>
		public static readonly BindableProperty SeparatorColorProperty = BindableProperty.Create("SeparatorColor", typeof(Color), typeof(ListView), null);

		/// <summary>Bindable property for <see cref="RefreshControlColor"/>.</summary>
		public static readonly BindableProperty RefreshControlColorProperty = BindableProperty.Create(nameof(RefreshControlColor), typeof(Color), typeof(ListView), null);

		/// <summary>Bindable property for <see cref="HorizontalScrollBarVisibility"/>.</summary>
		public static readonly BindableProperty HorizontalScrollBarVisibilityProperty = BindableProperty.Create(nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ListView), ScrollBarVisibility.Default);

		/// <summary>Bindable property for <see cref="VerticalScrollBarVisibility"/>.</summary>
		public static readonly BindableProperty VerticalScrollBarVisibilityProperty = BindableProperty.Create(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ListView), ScrollBarVisibility.Default);

		static readonly ToStringValueConverter _toStringValueConverter = new ToStringValueConverter();

		readonly Lazy<PlatformConfigurationRegistry<ListView>> _platformConfigurationRegistry;

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

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public ListView()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;
#pragma warning restore CS0618 // Type or member is obsolete

			TemplatedItems.IsGroupingEnabledProperty = IsGroupingEnabledProperty;
			TemplatedItems.GroupHeaderTemplateProperty = GroupHeaderTemplateProperty;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ListView>>(() => new PlatformConfigurationRegistry<ListView>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ListView([Parameter("CachingStrategy")] ListViewCachingStrategy cachingStrategy) : this()
		{
			// Unknown => UnitTest "platform"
			if (DeviceInfo.Platform == DevicePlatform.Unknown ||
				DeviceInfo.Platform == DevicePlatform.Android ||
				DeviceInfo.Platform == DevicePlatform.iOS ||
				DeviceInfo.Platform == DevicePlatform.macOS)
				CachingStrategy = cachingStrategy;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='Footer']/Docs/*" />
		public object Footer
		{
			get { return GetValue(FooterProperty); }
			set { SetValue(FooterProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='FooterTemplate']/Docs/*" />
		public DataTemplate FooterTemplate
		{
			get { return (DataTemplate)GetValue(FooterTemplateProperty); }
			set { SetValue(FooterTemplateProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			object bc = BindingContext;

			if (Header is Element header)
				SetChildInheritedBindingContext(header, bc);

			if (Footer is Element footer)
				SetChildInheritedBindingContext(footer, bc);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='GroupDisplayBinding']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='GroupHeaderTemplate']/Docs/*" />
		public DataTemplate GroupHeaderTemplate
		{
			get { return (DataTemplate)GetValue(GroupHeaderTemplateProperty); }
			set { SetValue(GroupHeaderTemplateProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='GroupShortNameBinding']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='HasUnevenRows']/Docs/*" />
		public bool HasUnevenRows
		{
			get { return (bool)GetValue(HasUnevenRowsProperty); }
			set { SetValue(HasUnevenRowsProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='Header']/Docs/*" />
		public object Header
		{
			get { return GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='HeaderTemplate']/Docs/*" />
		public DataTemplate HeaderTemplate
		{
			get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
			set { SetValue(HeaderTemplateProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='IsGroupingEnabled']/Docs/*" />
		public bool IsGroupingEnabled
		{
			get { return (bool)GetValue(IsGroupingEnabledProperty); }
			set { SetValue(IsGroupingEnabledProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='IsPullToRefreshEnabled']/Docs/*" />
		public bool IsPullToRefreshEnabled
		{
			get { return (bool)GetValue(IsPullToRefreshEnabledProperty); }
			set { SetValue(IsPullToRefreshEnabledProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='IsRefreshing']/Docs/*" />
		public bool IsRefreshing
		{
			get { return (bool)GetValue(IsRefreshingProperty); }
			set { SetValue(IsRefreshingProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='RefreshCommand']/Docs/*" />
		public ICommand RefreshCommand
		{
			get { return (ICommand)GetValue(RefreshCommandProperty); }
			set { SetValue(RefreshCommandProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='RowHeight']/Docs/*" />
		public int RowHeight
		{
			get { return (int)GetValue(RowHeightProperty); }
			set { SetValue(RowHeightProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='SelectedItem']/Docs/*" />
		public object SelectedItem
		{
			get { return GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='SelectionMode']/Docs/*" />
		public ListViewSelectionMode SelectionMode
		{
			get { return (ListViewSelectionMode)GetValue(SelectionModeProperty); }
			set { SetValue(SelectionModeProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='SeparatorColor']/Docs/*" />
		public Color SeparatorColor
		{
			get { return (Color)GetValue(SeparatorColorProperty); }
			set { SetValue(SeparatorColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='RefreshControlColor']/Docs/*" />
		public Color RefreshControlColor
		{
			get { return (Color)GetValue(RefreshControlColorProperty); }
			set { SetValue(RefreshControlColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='SeparatorVisibility']/Docs/*" />
		public SeparatorVisibility SeparatorVisibility
		{
			get { return (SeparatorVisibility)GetValue(SeparatorVisibilityProperty); }
			set { SetValue(SeparatorVisibilityProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='HorizontalScrollBarVisibility']/Docs/*" />
		public ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
			set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
		}
		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='VerticalScrollBarVisibility']/Docs/*" />
		public ScrollBarVisibility VerticalScrollBarVisibility
		{
			get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
			set { SetValue(VerticalScrollBarVisibilityProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='CachingStrategy']/Docs/*" />
		public ListViewCachingStrategy CachingStrategy { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='RefreshAllowed']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool RefreshAllowed
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

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='FooterElement']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Element FooterElement
		{
			get { return _footerElement; }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='HeaderElement']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Element HeaderElement
		{
			get { return _headerElement; }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='SendCellAppearing']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendCellAppearing(Cell cell)
			=> ItemAppearing?.Invoke(this, new ItemVisibilityEventArgs(cell.BindingContext, TemplatedItems.GetGlobalIndexOfItem(cell?.BindingContext)));

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='SendCellDisappearing']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendCellDisappearing(Cell cell)
			=> ItemDisappearing?.Invoke(this, new ItemVisibilityEventArgs(cell.BindingContext, TemplatedItems.GetGlobalIndexOfItem(cell?.BindingContext)));

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='SendScrolled']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendScrolled(ScrolledEventArgs args)
			=> Scrolled?.Invoke(this, args);

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='SendRefreshing']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendRefreshing()
		{
			BeginRefresh();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='BeginRefresh']/Docs/*" />
		public void BeginRefresh()
		{
			if (!RefreshAllowed)
				return;

			SetValue(IsRefreshingProperty, true);
			OnRefreshing(EventArgs.Empty);

			ICommand command = RefreshCommand;
			command?.Execute(null);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='EndRefresh']/Docs/*" />
		public void EndRefresh()
		{
			SetValue(IsRefreshingProperty, false);
		}

		public event EventHandler<ItemVisibilityEventArgs> ItemAppearing;

		public event EventHandler<ItemVisibilityEventArgs> ItemDisappearing;

		public event EventHandler<SelectedItemChangedEventArgs> ItemSelected;

		public event EventHandler<ItemTappedEventArgs> ItemTapped;

		public event EventHandler<ScrolledEventArgs> Scrolled;

		public event EventHandler Refreshing;

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='ScrollTo'][1]/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='ScrollTo'][2]/Docs/*" />
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
			TextCell textCell = new TextCell();
			textCell.SetBinding(TextCell.TextProperty, ".", converter: _toStringValueConverter);
			return textCell;
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			var minimumSize = new Size(40, 40);
			Size request;

			var scaled = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
			double width = Math.Min(scaled.Width, scaled.Height);

			var list = ItemsSource as IList;
			if (list != null && HasUnevenRows == false && RowHeight > 0 && !IsGroupingEnabled)
			{
				// we can calculate this
				request = new Size(width, list.Count * RowHeight);
			}
			else
			{
				// probably not worth it
				request = new Size(width, Math.Max(scaled.Width, scaled.Height));
			}

			return new SizeRequest(request, minimumSize);
		}

		protected override void SetupContent(Cell content, int index)
		{
			base.SetupContent(content, index);
			if (content is ViewCell viewCell && viewCell.View != null && HasUnevenRows)
				viewCell.View.ComputedConstraint = LayoutConstraint.None;

			if (content != null)
			{
				_visualChildren.Add(content);
				content.Parent = this;
				VisualDiagnostics.OnChildAdded(this, content);
			}
		}

		protected override void UnhookContent(Cell content)
		{
			base.UnhookContent(content);

			if (content == null)
				return;
			var index = _visualChildren.IndexOf(content);
			if (index == -1)
				return;
			_visualChildren.RemoveAt(index);
			content.Parent = null;
			VisualDiagnostics.OnChildRemoved(this, content, index);

		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='CreateDefaultCell']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Cell CreateDefaultCell(object item)
		{
			return CreateDefault(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='GetDisplayTextFromGroup']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string GetDisplayTextFromGroup(object cell)
		{
			int groupIndex = TemplatedItems.GetGlobalIndexOfGroup(cell);

			if (groupIndex == -1)
				return cell.ToString();

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

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='NotifyRowTapped'][2]/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void NotifyRowTapped(int groupIndex, int inGroupIndex, Cell cell = null)
		{
			var group = TemplatedItems.GetGroup(groupIndex);

			bool changed = _previousGroupSelected != groupIndex || _previousRowSelected != inGroupIndex;

			_previousRowSelected = inGroupIndex;
			_previousGroupSelected = groupIndex;

			// A11y: Keyboards and screen readers can deselect items, allowing -1 to be possible
			if (cell == null && inGroupIndex >= 0)
			{
				cell = group[inGroupIndex];
			}

			// Set SelectedItem before any events so we don't override any changes they may have made.
			if (SelectionMode != ListViewSelectionMode.None)
#pragma warning disable CS0618 // Type or member is obsolete
				SetValueCore(SelectedItemProperty, cell?.BindingContext, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearDynamicResource | (changed ? SetValueFlags.RaiseOnEqual : 0));
#pragma warning restore CS0618 // Type or member is obsolete

			cell?.OnTapped();

			ItemTapped?.Invoke(this, new ItemTappedEventArgs(ItemsSource.Cast<object>().ElementAt(groupIndex), cell?.BindingContext, TemplatedItems.GetGlobalIndexOfItem(cell?.BindingContext)));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='NotifyRowTapped'][4]/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void NotifyRowTapped(int groupIndex, int inGroupIndex, Cell cell, bool isContextMenuRequested)
		{
			var group = TemplatedItems.GetGroup(groupIndex);

			bool changed = _previousGroupSelected != groupIndex || _previousRowSelected != inGroupIndex;

			_previousRowSelected = inGroupIndex;
			_previousGroupSelected = groupIndex;

			// A11y: Keyboards and screen readers can deselect items, allowing -1 to be possible
			if (cell == null && inGroupIndex >= 0 && group.Count > inGroupIndex)
			{
				cell = group[inGroupIndex];
			}

			// Set SelectedItem before any events so we don't override any changes they may have made.
			if (SelectionMode != ListViewSelectionMode.None)
#pragma warning disable CS0618 // Type or member is obsolete
				SetValueCore(SelectedItemProperty, cell?.BindingContext, SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearDynamicResource | (changed ? SetValueFlags.RaiseOnEqual : 0));
#pragma warning restore CS0618 // Type or member is obsolete

			if (isContextMenuRequested || cell == null)
			{
				return;
			}

			cell.OnTapped();

			var itemSource = ItemsSource?.Cast<object>().ToList();
			object tappedGroup = null;
			if (itemSource?.Count > groupIndex)
			{
				tappedGroup = itemSource.ElementAt(groupIndex);
			}

			ItemTapped?.Invoke(this,
				new ItemTappedEventArgs(tappedGroup, cell.BindingContext,
					TemplatedItems.GetGlobalIndexOfItem(cell?.BindingContext)));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='NotifyRowTapped'][1]/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void NotifyRowTapped(int index, Cell cell = null)
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

		/// <include file="../../docs/Microsoft.Maui.Controls/ListView.xml" path="//Member[@MemberName='NotifyRowTapped'][3]/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void NotifyRowTapped(int index, Cell cell, bool isContextmenuRequested)
		{
			if (IsGroupingEnabled)
			{
				int leftOver;
				int groupIndex = TemplatedItems.GetGroupIndexFromGlobal(index, out leftOver);

				NotifyRowTapped(groupIndex, leftOver - 1, cell, isContextmenuRequested);
			}
			else
				NotifyRowTapped(0, index, cell, isContextmenuRequested);
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<ScrollToRequestedEventArgs> ScrollToRequested;

		void OnCommandCanExecuteChanged(object sender, EventArgs eventArgs)
		{
			RefreshAllowed = RefreshCommand != null && RefreshCommand.CanExecute(null);
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
				Application.Current?.FindMauiContext()?.CreateLogger<ListView>()?.LogWarning("GroupHeaderTemplate and GroupDisplayBinding cannot be set at the same time, setting GroupHeaderTemplate to null");
			}
		}

		static void OnGroupHeaderTemplateChanged(BindableObject bindable, object oldvalue, object newValue)
		{
			var lv = (ListView)bindable;
			if (newValue != null && lv.GroupDisplayBinding != null)
			{
				lv.GroupDisplayBinding = null;
				Application.Current?.FindMauiContext()?.CreateLogger<ListView>()?.LogWarning("GroupHeaderTemplate and GroupDisplayBinding cannot be set at the same time, setting GroupDisplayBinding to null");
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
			=> Refreshing?.Invoke(this, e);

		void OnScrollToRequested(ScrollToRequestedEventArgs e)
			=> ScrollToRequested?.Invoke(this, e);

		static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
			=> ((ListView)bindable).ItemSelected?.Invoke(bindable, new SelectedItemChangedEventArgs(newValue, ((ListView)bindable).TemplatedItems.GetGlobalIndexOfItem(newValue)));

		static bool ValidateHeaderFooterTemplate(BindableObject bindable, object value)
		{
			if (value == null)
				return true;
			var template = (DataTemplate)value;
			return template.CreateContent() is View;
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, ListView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		protected override bool ValidateItemTemplate(DataTemplate template)
		{
			var isRetainStrategy = CachingStrategy == ListViewCachingStrategy.RetainElement;
			var isDataTemplateSelector = ItemTemplate is DataTemplateSelector;
			if (isRetainStrategy && isDataTemplateSelector)
				return false;

			return true;
		}
	}
}
