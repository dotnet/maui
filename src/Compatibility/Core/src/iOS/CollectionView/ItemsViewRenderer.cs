using System.ComponentModel;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public abstract class ItemsViewRenderer<TItemsView, TViewController> : ViewRenderer<TItemsView, UIView>
		where TItemsView : ItemsView
		where TViewController : ItemsViewController<TItemsView>
	{
		ItemsViewLayout _layout;
		bool _disposed;
		bool? _defaultHorizontalScrollVisibility;
		bool? _defaultVerticalScrollVisibility;

		protected TItemsView ItemsView => Element;

		public override UIViewController ViewController => Controller;

		protected TViewController Controller { get; private set; }

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		protected ItemsViewRenderer()
		{
			AutoPackage = false;
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Control.GetSizeRequest(widthConstraint, heightConstraint, 0, 0);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TItemsView> e)
		{
			TearDownOldElement(e.OldElement);
			SetUpNewElement(e.NewElement);

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.Is(Microsoft.Maui.Controls.ItemsView.ItemsSourceProperty))
			{
				UpdateItemsSource();
			}
			else if (changedProperty.Is(Microsoft.Maui.Controls.ItemsView.ItemTemplateProperty))
			{
				UpdateLayout();
			}
			else if (changedProperty.IsOneOf(Microsoft.Maui.Controls.ItemsView.EmptyViewProperty,
				Microsoft.Maui.Controls.ItemsView.EmptyViewTemplateProperty))
			{
				Controller.UpdateEmptyView();
			}
			else if (changedProperty.Is(Microsoft.Maui.Controls.ItemsView.HorizontalScrollBarVisibilityProperty))
			{
				UpdateHorizontalScrollBarVisibility();
			}
			else if (changedProperty.Is(Microsoft.Maui.Controls.ItemsView.VerticalScrollBarVisibilityProperty))
			{
				UpdateVerticalScrollBarVisibility();
			}
			else if (changedProperty.Is(Microsoft.Maui.Controls.ItemsView.ItemsUpdatingScrollModeProperty))
			{
				UpdateItemsUpdatingScrollMode();
			}
			else if (changedProperty.Is(VisualElement.FlowDirectionProperty))
			{
				UpdateFlowDirection();
			}
			else if (changedProperty.Is(VisualElement.IsVisibleProperty))
			{
				UpdateVisibility();
			}
		}

		protected abstract ItemsViewLayout SelectLayout();

		protected virtual void TearDownOldElement(TItemsView oldElement)
		{
			if (oldElement == null)
			{
				return;
			}

			// Stop listening for ScrollTo requests
			oldElement.ScrollToRequested -= ScrollToRequested;
		}

		protected virtual void SetUpNewElement(TItemsView newElement)
		{
			if (newElement == null)
			{
				return;
			}

			UpdateLayout();
			Controller = CreateController(newElement, _layout);
			SetNativeControl(Controller.View);
			Controller.CollectionView.BackgroundColor = UIColor.Clear;
			UpdateHorizontalScrollBarVisibility();
			UpdateVerticalScrollBarVisibility();
			UpdateItemsUpdatingScrollMode();
			UpdateFlowDirection();
			UpdateVisibility();

			// Listen for ScrollTo requests
			newElement.ScrollToRequested += ScrollToRequested;
		}

		protected virtual void UpdateLayout()
		{
			_layout = SelectLayout();

			if (Controller != null)
			{
				Controller.UpdateLayout(_layout);
			}
		}

		protected virtual void UpdateItemSizingStrategy()
		{
			UpdateLayout();
		}

		protected virtual void UpdateItemsUpdatingScrollMode()
		{
			_layout.ItemsUpdatingScrollMode = ItemsView.ItemsUpdatingScrollMode;
		}

		protected virtual void UpdateFlowDirection()
		{
			if (Element == null)
			{
				return;
			}

			Controller.UpdateFlowDirection();
		}

		protected virtual void UpdateItemsSource()
		{
			UpdateItemsUpdatingScrollMode();
			Controller.UpdateItemsSource();
		}

		protected virtual void UpdateVisibility()
		{
			Controller?.UpdateVisibility();
		}

		protected abstract TViewController CreateController(TItemsView newElement, ItemsViewLayout layout);

		NSIndexPath DetermineIndex(ScrollToRequestEventArgs args)
		{
			if (args.Mode == ScrollToMode.Position)
			{
				if (args.GroupIndex == -1)
				{
					return NSIndexPath.Create(0, args.Index);
				}

				return NSIndexPath.Create(args.GroupIndex, args.Index);
			}

			return Controller.GetIndexForItem(args.Item);
		}

		void UpdateVerticalScrollBarVisibility()
		{
			if (_defaultVerticalScrollVisibility == null)
				_defaultVerticalScrollVisibility = Controller.CollectionView.ShowsVerticalScrollIndicator;

			switch (Element.VerticalScrollBarVisibility)
			{
				case ScrollBarVisibility.Always:
					Controller.CollectionView.ShowsVerticalScrollIndicator = true;
					break;
				case ScrollBarVisibility.Never:
					Controller.CollectionView.ShowsVerticalScrollIndicator = false;
					break;
				case ScrollBarVisibility.Default:
					Controller.CollectionView.ShowsVerticalScrollIndicator = _defaultVerticalScrollVisibility.Value;
					break;
			}
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			if (_defaultHorizontalScrollVisibility == null)
				_defaultHorizontalScrollVisibility = Controller.CollectionView.ShowsHorizontalScrollIndicator;

			switch (Element.HorizontalScrollBarVisibility)
			{
				case ScrollBarVisibility.Always:
					Controller.CollectionView.ShowsHorizontalScrollIndicator = true;
					break;
				case ScrollBarVisibility.Never:
					Controller.CollectionView.ShowsHorizontalScrollIndicator = false;
					break;
				case ScrollBarVisibility.Default:
					Controller.CollectionView.ShowsHorizontalScrollIndicator = _defaultHorizontalScrollVisibility.Value;
					break;
			}
		}

		protected virtual void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			using (var indexPath = DetermineIndex(args))
			{
				if (!IsIndexPathValid(indexPath))
				{
					// Specified path wasn't valid, or item wasn't found
					return;
				}

				Controller.CollectionView.ScrollToItem(indexPath,
					args.ScrollToPosition.ToCollectionViewScrollPosition(_layout.ScrollDirection), args.IsAnimated);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				TearDownOldElement(Element);

				Controller?.Dispose();
				Controller = null;
			}

			base.Dispose(disposing);
		}

		protected bool IsIndexPathValid(NSIndexPath indexPath)
		{
			if (indexPath.Item < 0 || indexPath.Section < 0)
			{
				return false;
			}

			var collectionView = Controller.CollectionView;
			if (indexPath.Section >= collectionView.NumberOfSections())
			{
				return false;
			}

			if (indexPath.Item >= collectionView.NumberOfItemsInSection(indexPath.Section))
			{
				return false;
			}

			return true;
		}
	}
}
