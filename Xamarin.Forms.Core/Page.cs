using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_PageRenderer))]
	public class Page : VisualElement, ILayout, IPageController, IElementConfiguration<Page>, IPaddingElement
	{
		public const string BusySetSignalName = "Xamarin.BusySet";

		public const string AlertSignalName = "Xamarin.SendAlert";

		public const string ActionSheetSignalName = "Xamarin.ShowActionSheet";

		internal static readonly BindableProperty IgnoresContainerAreaProperty = BindableProperty.Create("IgnoresContainerArea", typeof(bool), typeof(Page), false);

		public static readonly BindableProperty BackgroundImageProperty = BindableProperty.Create("BackgroundImage", typeof(string), typeof(Page), default(string));

		public static readonly BindableProperty IsBusyProperty = BindableProperty.Create("IsBusy", typeof(bool), typeof(Page), false, propertyChanged: (bo, o, n) => ((Page)bo).OnPageBusyChanged());

		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		public static readonly BindableProperty TitleProperty = BindableProperty.Create("Title", typeof(string), typeof(Page), null);

		public static readonly BindableProperty IconProperty = BindableProperty.Create("Icon", typeof(FileImageSource), typeof(Page), default(FileImageSource));

		readonly Lazy<PlatformConfigurationRegistry<Page>> _platformConfigurationRegistry;

		bool _allocatedFlag;
		Rectangle _containerArea;

		bool _containerAreaSet;

		bool _hasAppeared;

		ReadOnlyCollection<Element> _logicalChildren;

		View _titleView;

		public Page()
		{
			var toolbarItems = new ObservableCollection<ToolbarItem>();
			toolbarItems.CollectionChanged += OnToolbarItemsCollectionChanged;
			ToolbarItems = toolbarItems;
			InternalChildren.CollectionChanged += InternalChildrenOnCollectionChanged;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Page>>(() => new PlatformConfigurationRegistry<Page>(this));
		}

		public string BackgroundImage
		{
			get { return (string)GetValue(BackgroundImageProperty); }
			set { SetValue(BackgroundImageProperty, value); }
		}

		public FileImageSource Icon
		{
			get { return (FileImageSource)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public bool IsBusy
		{
			get { return (bool)GetValue(IsBusyProperty); }
			set { SetValue(IsBusyProperty, value); }
		}

		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingElement.PaddingProperty); }
			set { SetValue(PaddingElement.PaddingProperty, value); }
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return default(Thickness);
		}

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			UpdateChildrenLayout();
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public IList<ToolbarItem> ToolbarItems { get; internal set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Rectangle ContainerArea
		{
			get { return _containerArea; }
			set
			{
				if (_containerArea == value)
					return;
				_containerAreaSet = true;
				_containerArea = value;
				ForceLayout();
			}
		}

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IgnoresContainerArea
		{
			get { return (bool)GetValue(IgnoresContainerAreaProperty); }
			set { SetValue(IgnoresContainerAreaProperty, value); }
		}

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => 
			_logicalChildren ?? (_logicalChildren = new ReadOnlyCollection<Element>(InternalChildren));

		public event EventHandler LayoutChanged;

		public event EventHandler Appearing;

		public event EventHandler Disappearing;

		public Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
		{
			var args = new ActionSheetArguments(title, cancel, destruction, buttons);
			MessagingCenter.Send(this, ActionSheetSignalName, args);
			return args.Result.Task;
		}

		public Task DisplayAlert(string title, string message, string cancel)
		{
			return DisplayAlert(title, message, null, cancel);
		}

		public Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
		{
			if (string.IsNullOrEmpty(cancel))
				throw new ArgumentNullException("cancel");

			var args = new AlertArguments(title, message, accept, cancel);
			MessagingCenter.Send(this, AlertSignalName, args);
			return args.Result.Task;
		}

		public void ForceLayout()
		{
			SizeAllocated(Width, Height);
		}

		public bool SendBackButtonPressed()
		{
			return OnBackButtonPressed();
		}

		protected virtual void LayoutChildren(double x, double y, double width, double height)
		{
			var area = new Rectangle(x, y, width, height);
			Rectangle originalArea = area;
			if (_containerAreaSet)
			{
				area = ContainerArea;
				area.X += Padding.Left;
				area.Y += Padding.Right;
				area.Width -= Padding.HorizontalThickness;
				area.Height -= Padding.VerticalThickness;
				area.Width = Math.Max(0, area.Width);
				area.Height = Math.Max(0, area.Height);
			}

			List<Element> elements = LogicalChildren.ToList();
			foreach (Element element in elements)
			{
				var child = element as VisualElement;
				if (child == null)
					continue;
				var page = child as Page;
				if (page != null && page.IgnoresContainerArea)
					Forms.Layout.LayoutChildIntoBoundingRegion(child, originalArea);
				else
					Forms.Layout.LayoutChildIntoBoundingRegion(child, area);
			}
		}

		protected virtual void OnAppearing()
		{
		}

		protected virtual bool OnBackButtonPressed()
		{
			var application = RealParent as Application;
			if (application == null || this == application.MainPage)
				return false;

			var canceled = false;
			EventHandler handler = (sender, args) => { canceled = true; };
			application.PopCanceled += handler;
			Navigation.PopModalAsync().ContinueWith(t => { throw t.Exception; }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext());

			application.PopCanceled -= handler;
			return !canceled;
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			foreach (ToolbarItem toolbarItem in ToolbarItems)
			{
				SetInheritedBindingContext(toolbarItem, BindingContext);
			}

			if(_titleView != null)
				SetInheritedBindingContext(_titleView, BindingContext);
		}

		protected virtual void OnChildMeasureInvalidated(object sender, EventArgs e)
		{
			InvalidationTrigger trigger = (e as InvalidationEventArgs)?.Trigger ?? InvalidationTrigger.Undefined;
			OnChildMeasureInvalidated((VisualElement)sender, trigger);
		}

		protected virtual void OnDisappearing()
		{
		}

		protected override void OnParentSet()
		{
			if (!Application.IsApplicationOrNull(RealParent) && !(RealParent is Page))
				throw new InvalidOperationException("Parent of a Page must also be a Page");
			base.OnParentSet();
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			_allocatedFlag = true;
			base.OnSizeAllocated(width, height);
			UpdateChildrenLayout();
		}

		protected void UpdateChildrenLayout()
		{
			if (!ShouldLayoutChildren())
				return;

			var startingLayout = new List<Rectangle>(LogicalChildren.Count);
			foreach (VisualElement c in LogicalChildren)
			{
				startingLayout.Add(c.Bounds);
			}

			double x = Padding.Left;
			double y = Padding.Top;
			double w = Math.Max(0, Width - Padding.HorizontalThickness);
			double h = Math.Max(0, Height - Padding.VerticalThickness);

			LayoutChildren(x, y, w, h);

			for (var i = 0; i < LogicalChildren.Count; i++)
			{
				var c = (VisualElement)LogicalChildren[i];

				if (c.Bounds != startingLayout[i])
				{
					LayoutChanged?.Invoke(this, EventArgs.Empty);
					return;
				}
			}
		}

		internal virtual void OnChildMeasureInvalidated(VisualElement child, InvalidationTrigger trigger)
		{
			var container = this as IPageContainer<Page>;
			if (container != null)
			{
				Page page = container.CurrentPage;
				if (page != null && page.IsVisible && (!page.IsPlatformEnabled || !page.IsNativeStateConsistent))
					return;
			}
			else
			{
				for (var i = 0; i < LogicalChildren.Count; i++)
				{
					var v = LogicalChildren[i] as VisualElement;
					if (v != null && v.IsVisible && (!v.IsPlatformEnabled || !v.IsNativeStateConsistent))
						return;
				}
			}

			_allocatedFlag = false;
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
			if (!_allocatedFlag && Width >= 0 && Height >= 0)
			{
				SizeAllocated(Width, Height);
			}
		}

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendAppearing()
		{
			if (_hasAppeared)
				return;

			_hasAppeared = true;

			if (IsBusy)
				MessagingCenter.Send(this, BusySetSignalName, true);

			OnAppearing();
			Appearing?.Invoke(this, EventArgs.Empty);

			var pageContainer = this as IPageContainer<Page>;
			pageContainer?.CurrentPage?.SendAppearing();

			FindApplication(this)?.OnPageAppearing(this);
		}

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendDisappearing()
		{
			if (!_hasAppeared)
				return;

			_hasAppeared = false;

			if (IsBusy)
				MessagingCenter.Send(this, BusySetSignalName, false);

			var pageContainer = this as IPageContainer<Page>;
			pageContainer?.CurrentPage?.SendDisappearing();

			OnDisappearing();
			Disappearing?.Invoke(this, EventArgs.Empty);

			FindApplication(this)?.OnPageDisappearing(this);
		}

		Application FindApplication(Element element)
		{
			if (element == null)
				return null;

			return (element.Parent is Application app) ? app : FindApplication(element.Parent);
		}

		void InternalChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (VisualElement item in e.OldItems.OfType<VisualElement>())
					OnInternalRemoved(item);
			}

			if (e.NewItems != null)
			{
				foreach (VisualElement item in e.NewItems.OfType<VisualElement>())
					OnInternalAdded(item);
			}
		}

		void OnInternalAdded(VisualElement view)
		{
			view.MeasureInvalidated += OnChildMeasureInvalidated;

			OnChildAdded(view);
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		void OnInternalRemoved(VisualElement view)
		{
			view.MeasureInvalidated -= OnChildMeasureInvalidated;

			OnChildRemoved(view);
		}

		void OnPageBusyChanged()
		{
			if (!_hasAppeared)
				return;

			MessagingCenter.Send(this, BusySetSignalName, IsBusy);
		}

		void OnToolbarItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action != NotifyCollectionChangedAction.Add)
				return;
			foreach (IElement item in args.NewItems)
				item.Parent = this;
		}

		bool ShouldLayoutChildren()
		{
			if (!LogicalChildren.Any() || Width <= 0 || Height <= 0 || !IsNativeStateConsistent)
				return false;

			var container = this as IPageContainer<Page>;
			if (container?.CurrentPage != null)
			{
				if (InternalChildren.Contains(container.CurrentPage))
					return container.CurrentPage.IsPlatformEnabled && container.CurrentPage.IsNativeStateConsistent;
				return true;
			}

			var any = false;
			for (var i = 0; i < LogicalChildren.Count; i++)
			{
				var v = LogicalChildren[i] as VisualElement;
				if (v != null && (!v.IsPlatformEnabled || !v.IsNativeStateConsistent))
				{
					any = true;
					break;
				}
			}
			return !any;
		}

		public IPlatformElementConfiguration<T, Page> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		internal void SetTitleView(View oldTitleView, View newTitleView)
		{
			if (oldTitleView != null)
				oldTitleView.Parent = null;

			if (newTitleView != null)
				newTitleView.Parent = this;

			_titleView = newTitleView;
		}
	}
}
