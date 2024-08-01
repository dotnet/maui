#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using WASDKApp = Microsoft.UI.Xaml.Application;
using WASDKDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using WASDKScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;
using WVisibility = Microsoft.UI.Xaml.Visibility;
using WRect = Windows.Foundation.Rect;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract partial class ItemsViewHandler<TItemsView> : ViewHandler<TItemsView, WItemsView> where TItemsView : ItemsView
	{
		protected CollectionViewSource CollectionViewSource;
		FrameworkElement _emptyView;
		WASDKScrollBarVisibility? _defaultHorizontalScrollVisibility;
		WASDKScrollBarVisibility? _defaultVerticalScrollVisibility;
		View _formsEmptyView;
		bool _emptyViewDisplayed;
		double _previousHorizontalOffset;
		double _previousVerticalOffset;
		IList _itemsSource;

		protected WItemsView ListViewBase => PlatformView;
		protected TItemsView ItemsView => VirtualView;
		protected TItemsView Element => VirtualView;
		protected WASDKDataTemplate ViewTemplate => (WASDKDataTemplate)WASDKApp.Current.Resources["View"];
		protected WASDKDataTemplate ItemsViewTemplate => (WASDKDataTemplate)WASDKApp.Current.Resources["ItemsViewDefaultTemplate"];

		UIElement Control => PlatformView;

		protected abstract IItemsLayout Layout { get; }

		protected override WItemsView CreatePlatformView()
		{
			var itemsView = SelectListViewBase();
			itemsView.ItemTransitionProvider = new DefaultItemCollectionTransitionProvider();
			return itemsView;
		}

		protected override void ConnectHandler(WItemsView platformView)
		{
			base.ConnectHandler(platformView);
			VirtualView.ScrollToRequested += ScrollToRequested;
			FindScrollViewer(ListViewBase);
		}

		protected override void DisconnectHandler(WItemsView platformView)
		{
			VirtualView.ScrollToRequested -= ScrollToRequested;
			base.DisconnectHandler(platformView);
		}

		public static void MapItemsSource(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateItemsSource();
		}

		public static void MapHorizontalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateHorizontalScrollBarVisibility();
		}

		public static void MapVerticalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateVerticalScrollBarVisibility();
		}

		public static void MapItemTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateItemTemplate();
		}

		public static void MapEmptyView(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateEmptyView();
		}

		public static void MapEmptyViewTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateEmptyView();
		}

		public static void MapFlowDirection(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.PlatformView.UpdateFlowDirection(itemsView);
		}

		public static void MapIsVisible(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.PlatformView.UpdateVisibility(itemsView);
		}

		public static void MapItemsUpdatingScrollMode(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		protected abstract WItemsView SelectListViewBase();

		protected virtual void CleanUpCollectionViewSource()
		{
			if (CollectionViewSource is not null)
			{
				if (CollectionViewSource.Source is ObservableItemTemplateCollection observableItemTemplateCollection)
				{
					observableItemTemplateCollection.CleanUp();
				}

				if (CollectionViewSource.Source is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= ItemsChanged;
				}

				CollectionViewSource.Source = null;
				CollectionViewSource = null;
			}

			// Remove all children inside the ItemsSource
			if (VirtualView is not null)
			{
				foreach (var item in ListViewBase.GetChildren<ItemContentControl>())
				{
					var element = item.GetVisualElement();
					VirtualView.RemoveLogicalChild(element);
				}
			}

			if (VirtualView?.ItemsSource is null)
			{
				ListViewBase.ItemsSource = null;
				return;
			}
		}

		void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateEmptyViewVisibility();

			if (_itemsSource is null)
				return;

			if (VirtualView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepItemsInView)
			{
				// Keeps the first item in the list displayed when new items are added.
				ListViewBase.StartBringItemIntoView(0, new BringIntoViewOptions() { AnimationDesired = false });
			}
			else if (VirtualView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepLastItemInView)
			{
				// Adjusts the scroll offset to keep the last item in the list displayed when new items are added.
				ListViewBase.StartBringItemIntoView(_itemsSource.Count - 1, new BringIntoViewOptions() { AnimationDesired = false });
			}
		}

		protected virtual void UpdateEmptyViewVisibility()
		{
			bool isEmpty = (CollectionViewSource?.View?.Count ?? 0) == 0;

			if (isEmpty)
			{
				if (_formsEmptyView != null)
				{
					if (_emptyViewDisplayed)
						ItemsView.RemoveLogicalChild(_formsEmptyView);

					if (ItemsView.EmptyViewTemplate == null)
						ItemsView.AddLogicalChild(_formsEmptyView);
				}

				if (_emptyView != null && ListViewBase is IEmptyView emptyView)
				{
					emptyView.EmptyViewVisibility = WVisibility.Visible;

					if (PlatformView.ActualWidth >= 0 && PlatformView.ActualHeight >= 0)
						_formsEmptyView?.Layout(new Rect(0, 0, PlatformView.ActualWidth, PlatformView.ActualHeight));
				}

				_emptyViewDisplayed = true;
			}
			else
			{
				if (_emptyViewDisplayed)
				{
					if (_emptyView != null && ListViewBase is IEmptyView emptyView)
						emptyView.EmptyViewVisibility = WVisibility.Collapsed;

					ItemsView.RemoveLogicalChild(_formsEmptyView);
				}

				_emptyViewDisplayed = false;
			}
		}

		protected virtual void UpdateItemsSource()
		{
			if (ListViewBase == null)
			{
				return;
			}

			CleanUpCollectionViewSource();

			if (Element.ItemsSource == null)
			{
				return;
			}

			CollectionViewSource = CreateCollectionViewSource();
			_itemsSource = CollectionViewSource?.Source as IList;

			if (CollectionViewSource?.Source is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += ItemsChanged;
			}

			ListViewBase.ItemsSource = GetCollectionView(CollectionViewSource);
			ListViewBase.ItemTemplate = new ItemFactory(Element);

			UpdateEmptyViewVisibility();
		}

		protected virtual void UpdateItemTemplate()
		{
			if (Element == null || ListViewBase == null)
			{
				return;
			}

			ListViewBase.ItemTemplate = Element.ItemTemplate == null ? null : ItemsViewTemplate;

			UpdateItemsSource();
		}

		protected virtual CollectionViewSource CreateCollectionViewSource()
		{
			var itemsSource = Element.ItemsSource;
			var itemTemplate = Element.ItemTemplate;

			if (itemTemplate != null)
			{
				return new CollectionViewSource
				{
					Source = TemplatedItemSourceFactory.Create(itemsSource, itemTemplate, Element, mauiContext: MauiContext),
					IsSourceGrouped = false
				};
			}

			return new CollectionViewSource
			{
				Source = itemsSource,
				IsSourceGrouped = false
			};
		}

		protected virtual ICollectionView GetCollectionView(CollectionViewSource collectionViewSource)
		{
			return collectionViewSource.View;
		}

		protected virtual void UpdateEmptyView()
		{
			if (Element == null || ListViewBase == null)
			{
				return;
			}

			var emptyView = Element.EmptyView;

			if (emptyView == null)
			{
				return;
			}

			switch (emptyView)
			{
				case string text:
					_emptyView = new TextBlock
					{
						HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
						VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
						Text = text
					};
					break;
				case View view:
					_emptyView = RealizeEmptyView(view);
					break;
				default:
					_emptyView = RealizeEmptyViewTemplate(emptyView, Element.EmptyViewTemplate);
					break;
			}

			(ListViewBase as IEmptyView)?.SetEmptyView(_emptyView, _formsEmptyView);

			UpdateEmptyViewVisibility();
		}

		protected virtual void UpdateItemsLayout()
		{
			FindScrollViewer(ListViewBase);

			_defaultHorizontalScrollVisibility = null;
			_defaultVerticalScrollVisibility = null;

			UpdateItemTemplate();
			UpdateItemsSource();
			UpdateVerticalScrollBarVisibility();
			UpdateHorizontalScrollBarVisibility();
			UpdateEmptyView();
		}

		void FindScrollViewer(WItemsView listView)
		{
			if (ListViewBase.ScrollView != null)
			{
				OnScrollViewerFound();
				return;
			}

			void ListViewLoaded(object sender, RoutedEventArgs e)
			{
				var lv = (WItemsView)sender;
				lv.Loaded -= ListViewLoaded;
				FindScrollViewer(listView);
			}

			listView.Loaded += ListViewLoaded;
		}

		void UpdateVerticalScrollBarVisibility()
		{
			if (Element.VerticalScrollBarVisibility != ScrollBarVisibility.Default)
			{
				// If the value is changing to anything other than the default, record the default 
				if (_defaultVerticalScrollVisibility == null)
					_defaultVerticalScrollVisibility = ScrollViewer.GetVerticalScrollBarVisibility(Control);
			}

			if (_defaultVerticalScrollVisibility == null)
			{
				// If the default has never been recorded, then this has never been set to anything but the 
				// default value; there's nothing to do.
				return;
			}

			switch (Element.VerticalScrollBarVisibility)
			{
				case (ScrollBarVisibility.Always):
					ScrollViewer.SetVerticalScrollBarVisibility(Control, WASDKScrollBarVisibility.Visible);
					break;
				case (ScrollBarVisibility.Never):
					ScrollViewer.SetVerticalScrollBarVisibility(Control, WASDKScrollBarVisibility.Hidden);
					break;
				case (ScrollBarVisibility.Default):
					ScrollViewer.SetVerticalScrollBarVisibility(Control, _defaultVerticalScrollVisibility.Value);
					break;
			}
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			if (_defaultHorizontalScrollVisibility == null)
				_defaultHorizontalScrollVisibility = ScrollViewer.GetHorizontalScrollBarVisibility(Control);

			switch (Element.HorizontalScrollBarVisibility)
			{
				case (ScrollBarVisibility.Always):
					ScrollViewer.SetHorizontalScrollBarVisibility(Control, WASDKScrollBarVisibility.Visible);
					break;
				case (ScrollBarVisibility.Never):
					ScrollViewer.SetHorizontalScrollBarVisibility(Control, WASDKScrollBarVisibility.Hidden);
					break;
				case (ScrollBarVisibility.Default):
					ScrollViewer.SetHorizontalScrollBarVisibility(Control, _defaultHorizontalScrollVisibility.Value);
					break;
			}
		}

		protected virtual void OnScrollViewerFound()
		{
			if (ListViewBase.ScrollView != null)
			{
				ListViewBase.ScrollView.ViewChanged -= ScrollViewChanged;
				ListViewBase.ScrollView.PointerWheelChanged -= PointerScrollChanged;
			}

			ListViewBase.ScrollView.ViewChanged += ScrollViewChanged;
			ListViewBase.ScrollView.PointerWheelChanged += PointerScrollChanged;
		}

		void ScrollViewChanged(UI.Xaml.Controls.ScrollView sender, object args)
		{
			HandleScroll(ListViewBase.ScrollView.ScrollPresenter);
		}

		private void PointerScrollChanged(object sender, PointerRoutedEventArgs e)
		{
			if (ListViewBase.ScrollView.ComputedHorizontalScrollMode == ScrollingScrollMode.Enabled)
			{
				ListViewBase.ScrollView.AddScrollVelocity(new(e.GetCurrentPoint(ListViewBase.ScrollView).Properties.MouseWheelDelta / -2.5f, 0), null);
			}
		}

		FrameworkElement RealizeEmptyViewTemplate(object bindingContext, DataTemplate emptyViewTemplate)
		{
			if (emptyViewTemplate == null)
			{
				return new TextBlock
				{
					HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
					VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
					Text = bindingContext.ToString()
				};
			}

			var template = emptyViewTemplate.SelectDataTemplate(bindingContext, null);
			var view = template.CreateContent() as View;
			view.BindingContext = bindingContext;

			return RealizeEmptyView(view);
		}

		FrameworkElement RealizeEmptyView(View view)
		{
			_formsEmptyView = view ?? throw new ArgumentNullException(nameof(view));

			var handler = view.ToHandler(MauiContext);
			var platformView = handler.ContainerView ?? handler.PlatformView;

			return platformView as FrameworkElement;
		}

		internal void HandleScroll(ScrollPresenter scrollViewer)
		{
			var itemsViewScrolledEventArgs = new ItemsViewScrolledEventArgs
			{
				HorizontalOffset = scrollViewer.HorizontalOffset,
				HorizontalDelta = scrollViewer.HorizontalOffset - _previousHorizontalOffset,
				VerticalOffset = scrollViewer.VerticalOffset,
				VerticalDelta = scrollViewer.VerticalOffset - _previousVerticalOffset,
			};

			_previousHorizontalOffset = scrollViewer.HorizontalOffset;
			_previousVerticalOffset = scrollViewer.VerticalOffset;

			bool advancing = true;
			switch (Layout)
			{
				case LinearItemsLayout linearItemsLayout:
					advancing = itemsViewScrolledEventArgs.HorizontalDelta > 0;
					break;
				case GridItemsLayout gridItemsLayout:
					advancing = itemsViewScrolledEventArgs.VerticalDelta > 0;
					break;
				default:
					break;
			}

			itemsViewScrolledEventArgs = ComputeVisibleIndexes(itemsViewScrolledEventArgs, advancing);

			Element.SendScrolled(itemsViewScrolledEventArgs);

			var remainingItemsThreshold = Element.RemainingItemsThreshold;
			if (remainingItemsThreshold > -1 &&
				ItemCount - 1 - itemsViewScrolledEventArgs.LastVisibleItemIndex <= remainingItemsThreshold)
			{
				Element.SendRemainingItemsThresholdReached();
			}
		}

		protected virtual ItemsViewScrolledEventArgs ComputeVisibleIndexes(ItemsViewScrolledEventArgs args, bool advancing)
		{
			var (firstVisibleItemIndex, lastVisibleItemIndex, centerItemIndex) = GetVisibleIndexes(advancing);

			args.FirstVisibleItemIndex = firstVisibleItemIndex;
			args.CenterItemIndex = centerItemIndex;
			args.LastVisibleItemIndex = lastVisibleItemIndex;

			return args;
		}

		(int firstVisibleItemIndex, int lastVisibleItemIndex, int centerItemIndex) GetVisibleIndexes(bool advancing)
		{
			int firstVisibleItemIndex = -1;
			int lastVisibleItemIndex = -1;

			ListViewBase.TryGetItemIndex(0, 0, out firstVisibleItemIndex);
			ListViewBase.TryGetItemIndex(1, 1, out lastVisibleItemIndex);

			double center = (lastVisibleItemIndex + firstVisibleItemIndex) / 2.0;
			int centerItemIndex = advancing ? (int)Math.Ceiling(center) : (int)Math.Floor(center);

			return (firstVisibleItemIndex, lastVisibleItemIndex, centerItemIndex);
		}

		void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			var index = args.Index;
			if (args.Mode == ScrollToMode.Element)
			{
				for (int i = 0; i < ItemCount; i++)
				{
					if (CollectionViewSource.View[i] is ItemTemplateContext pair)
					{
						if (pair.Item == args.Item)
						{
							index = i;
							break;
						}
					}
				}
			}

			//if (index >= 0)
			//{
			//	float offset = 0.0f;
			//	switch (args.ScrollToPosition)
			//	{
			//		case ScrollToPosition.Start:
			//			offset = 0.0f;
			//			break;
			//		case ScrollToPosition.Center:
			//			offset = 0.5f;
			//			break;
			//		case ScrollToPosition.End:
			//			offset = 1.0f;
			//			break;
			//	}
			//
			//	this.ListViewBase.StartBringItemIntoView(index, new BringIntoViewOptions()
			//	{
			//		AnimationDesired = args.IsAnimated,
			//		VerticalAlignmentRatio = offset,
			//		HorizontalAlignmentRatio = offset
			//	});
			//}
		}


		protected virtual int ItemCount => CollectionViewSource.View.Count;

		protected virtual object GetItem(int index)
		{
			return CollectionViewSource.View[index];
		}
	}

	class ItemFactory(ItemsView view) : IElementFactory
	{
		private readonly ItemsView _view = view;
		private readonly RecyclePool _recyclePool = new();

		public UIElement GetElement(ElementFactoryGetArgs args)
		{
			// NOTE: 1.6: replace w/ RecyclePool
			if (args.Data is ItemTemplateContext templateContext)
			{
				Microsoft.Maui.Controls.DataTemplate template = templateContext.FormsDataTemplate;
				if (template is Microsoft.Maui.Controls.DataTemplateSelector selector)
				{
					template = selector.SelectTemplate(templateContext.Item, _view);
				}

				if (template is null)
				{
					template = _view.EmptyViewTemplate;
				}

				ItemContainer container = null;
				ElementWrapper wrapper = null;
				var pool = RecyclePool.GetPoolInstance(template);
				if (pool is not null)
				{
					container = pool.TryGetElement(string.Empty, args.Parent) as ItemContainer;
					if (container is not null)
					{
						wrapper = container.Child as ElementWrapper;
					}
				}

				if (wrapper is null)
				{
					var viewContent = template.CreateContent() as View;
					wrapper = new ElementWrapper(_view.Handler.MauiContext);
					wrapper.SetContent(viewContent);

					((View)wrapper.VirtualView).SetValue(RecyclePool.OriginTemplateProperty, template);
				}

				if (wrapper.VirtualView is View view)
				{
					view.BindingContext = templateContext.Item ?? _view.BindingContext;
					_view.AddLogicalChild(view);
				}

				container ??= new ItemContainer()
				{
					Child = wrapper,
					IsEnabled = !templateContext.IsHeader && !templateContext.IsFooter
					// CanUserSelect = !templateContext.IsHeader // 1.6 feature
				};
				return container;

			}
			return null;
		}

		public void RecycleElement(ElementFactoryRecycleArgs args)
		{
			var item = args.Element as ItemContainer;
			var wrapper = item.Child as ElementWrapper;
			var wrapperView = wrapper.VirtualView as View;
			Microsoft.Maui.Controls.DataTemplate template =
				wrapperView.GetValue(RecyclePool.OriginTemplateProperty) as Microsoft.Maui.Controls.DataTemplate;

			var recyclePool = RecyclePool.GetPoolInstance(template);
			if (recyclePool == null)
			{
				// No Recycle pool in the template, create one.
				recyclePool = new RecyclePool();
				RecyclePool.SetPoolInstance(template, recyclePool);
			}
			recyclePool.PutElement(element: item, key: string.Empty, owner: args.Parent);
			_view.RemoveLogicalChild(wrapperView);
		}
	}

	class ElementWrapper(IMauiContext context) : UserControl
	{
		private IMauiContext _context = context;
		public IView VirtualView { get; private set; }

		public void SetContent(IView view)
		{
			if (VirtualView is null || VirtualView.Handler is null)
			{
				Content = view.ToPlatform(_context);
				VirtualView = view;
			}
		}
	}

	/// <summary>
	///  From the Repeater WinAppSDK test UI sample
	/// </summary>
	class DefaultItemCollectionTransitionProvider : ItemCollectionTransitionProvider
	{
		private const double DefaultAnimationDurationInMs = 150.0;
		public static double AnimationSlowdownFactor { get; set; }

		static DefaultItemCollectionTransitionProvider()
		{
			AnimationSlowdownFactor = 1.0;
		}

		protected override bool ShouldAnimateCore(ItemCollectionTransition transition)
		{
			return true;
		}

		protected override void StartTransitions(IList<ItemCollectionTransition> transitions)
		{
			var addTransitions = transitions.Where(transition => transition.Operation == ItemCollectionTransitionOperation.Add).ToList();
			var removeTransitions = transitions.Where(transition => transition.Operation == ItemCollectionTransitionOperation.Remove).ToList();
			var moveTransitions = transitions.Where(transition => transition.Operation == ItemCollectionTransitionOperation.Move).ToList();

			StartAddTransitions(addTransitions, removeTransitions.Count > 0, moveTransitions.Count > 0);
			StartRemoveTransitions(removeTransitions);
			StartMoveTransitions(moveTransitions, removeTransitions.Count > 0);
		}

		private static void StartAddTransitions(IList<ItemCollectionTransition> transitions, bool hasRemoveTransitions, bool hasMoveTransitions)
		{
			foreach (var transition in transitions)
			{
				var progress = transition.Start();
				var visual = ElementCompositionPreview.GetElementVisual(progress.Element);
				var compositor = visual.Compositor;

				var fadeInAnimation = compositor.CreateScalarKeyFrameAnimation();
				fadeInAnimation.InsertKeyFrame(0.0f, 0.0f);

				if (hasMoveTransitions && hasRemoveTransitions)
				{
					fadeInAnimation.InsertKeyFrame(0.66f, 0.0f);
				}
				else if (hasMoveTransitions || hasRemoveTransitions)
				{
					fadeInAnimation.InsertKeyFrame(0.5f, 0.0f);
				}

				fadeInAnimation.InsertKeyFrame(1.0f, 1.0f);
				fadeInAnimation.Duration = TimeSpan.FromMilliseconds(
					DefaultAnimationDurationInMs * ((hasRemoveTransitions ? 1 : 0) + (hasMoveTransitions ? 1 : 0) + 1) * AnimationSlowdownFactor);

				var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
				visual.StartAnimation("Opacity", fadeInAnimation);
				batch.End();
				batch.Completed += delegate { progress.Complete(); };
			}
		}

		private static void StartRemoveTransitions(IList<ItemCollectionTransition> transitions)
		{
			foreach (var transition in transitions)
			{
				var progress = transition.Start();
				var visual = ElementCompositionPreview.GetElementVisual(progress.Element);
				var compositor = visual.Compositor;

				var fadeOutAnimation = compositor.CreateScalarKeyFrameAnimation();
				fadeOutAnimation.InsertExpressionKeyFrame(0.0f, "this.CurrentValue");
				fadeOutAnimation.InsertKeyFrame(1.0f, 0.0f);
				fadeOutAnimation.Duration = TimeSpan.FromMilliseconds(DefaultAnimationDurationInMs * AnimationSlowdownFactor);

				var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
				visual.StartAnimation("Opacity", fadeOutAnimation);
				batch.End();
				batch.Completed += delegate {
					visual.Opacity = 1.0f;
					progress.Complete();
				};
			}
		}

		private static void StartMoveTransitions(IList<ItemCollectionTransition> transitions, bool hasRemoveAnimations)
		{
			foreach (var transition in transitions)
			{
				var progress = transition.Start();
				var visual = ElementCompositionPreview.GetElementVisual(progress.Element);
				var compositor = visual.Compositor;
				var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

				// Animate offset.
				if (transition.OldBounds.X != transition.NewBounds.X ||
					transition.OldBounds.Y != transition.NewBounds.Y)
				{
					AnimateOffset(visual, compositor, transition.OldBounds, transition.NewBounds, hasRemoveAnimations);
				}

				batch.End();
				batch.Completed += delegate { progress.Complete(); };
			}
		}

		private static void AnimateOffset(Visual visual, Compositor compositor, WRect oldBounds, WRect newBounds, bool hasRemoveAnimations)
		{
			var offsetAnimation = compositor.CreateVector2KeyFrameAnimation();

			offsetAnimation.SetVector2Parameter("start", new System.Numerics.Vector2(
				(float)(oldBounds.X - newBounds.X),
				(float)(oldBounds.Y - newBounds.Y)));
			offsetAnimation.SetVector2Parameter("end", new System.Numerics.Vector2());
			offsetAnimation.InsertExpressionKeyFrame(0.0f, "start");
			offsetAnimation.InsertExpressionKeyFrame(1.0f, "end");
			offsetAnimation.Duration = TimeSpan.FromMilliseconds(
				DefaultAnimationDurationInMs * AnimationSlowdownFactor);

			if (hasRemoveAnimations)
			{
				offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(100.0 * AnimationSlowdownFactor);
				offsetAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
			}

			visual.StartAnimation("TransformMatrix._41_42", offsetAnimation);
		}
	}
}
