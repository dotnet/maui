#nullable disable
using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class TemplatedCell2 : ItemsViewCell2
	{
		internal const string ReuseId = "Microsoft.Maui.Controls.TemplatedCell2";

		readonly WeakEventManager _weakEventManager = new();

		public event EventHandler<EventArgs> ContentSizeChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		public event EventHandler<LayoutAttributesChangedEventArgs2> LayoutAttributesChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		protected CGSize ConstrainedSize;

		protected nfloat ConstrainedDimension;

		WeakReference<DataTemplate> _currentTemplate;

		public DataTemplate CurrentTemplate
		{
			get => _currentTemplate is not null && _currentTemplate.TryGetTarget(out var target) ? target : null;
			private set => _currentTemplate = value is null ? null : new(value);
		}

		// Keep track of the cell size so we can verify whether a measure invalidation 
		// actually changed the size of the cell
		//Size _size;

		//internal CGSize CurrentSize => _size.ToCGSize();

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public TemplatedCell2(CGRect frame) : base(frame)
		{

		}

		public UICollectionViewScrollDirection ScrollDirection { get; set; }

		internal IPlatformViewHandler PlatformHandler { get; set; }

		internal UIView PlatformView { get; set; }

		internal void Unbind()
		{
			if (PlatformHandler?.VirtualView is View view)
			{
				//view.MeasureInvalidated -= MeasureInvalidated;
				view.BindingContext = null;
				(view.Parent as ItemsView)?.RemoveLogicalChild(view);
			}
		}

		public override UICollectionViewLayoutAttributes PreferredLayoutAttributesFittingAttributes(
			UICollectionViewLayoutAttributes layoutAttributes)
		{
			var preferredAttributes = base.PreferredLayoutAttributesFittingAttributes(layoutAttributes);

			if (PlatformHandler?.VirtualView is not null)
			{
				if (ScrollDirection == UICollectionViewScrollDirection.Vertical)
				{
					var measure =
						PlatformHandler.VirtualView.Measure(preferredAttributes.Size.Width, double.PositiveInfinity);

					preferredAttributes.Frame =
						new CGRect(preferredAttributes.Frame.X, preferredAttributes.Frame.Y,
							preferredAttributes.Frame.Width, measure.Height);
				}
				else
				{
					var measure =
						PlatformHandler.VirtualView.Measure(double.PositiveInfinity, preferredAttributes.Size.Height);

					preferredAttributes.Frame =
						new CGRect(preferredAttributes.Frame.X, preferredAttributes.Frame.Y,
							measure.Width, preferredAttributes.Frame.Height);
				}

				preferredAttributes.ZIndex = 2;
			}

			return preferredAttributes;
		}

		public override void PrepareForReuse()
		{
			//Unbind();
			base.PrepareForReuse();
		}

		public void Bind(DataTemplate template, object bindingContext, ItemsView itemsView)
		{
			var virtualView = template.CreateContent(bindingContext, itemsView) as View;
			BindVirtualView(virtualView, bindingContext, itemsView, false);
		}

		public void Bind(View virtualView, ItemsView itemsView)
		{
			BindVirtualView(virtualView, itemsView.BindingContext, itemsView, true);
		}

		void BindVirtualView(View virtualView, object bindingContext, ItemsView itemsView, bool needsContainer)
		{
			if (PlatformHandler is null && virtualView is not null)
			{
				var mauiContext = itemsView.FindMauiContext()!;
				var nativeView = virtualView.ToPlatform(mauiContext);

				if (needsContainer)
				{
					PlatformView = new GeneralWrapperView(virtualView, mauiContext);
				}
				else
				{
					PlatformView = nativeView;
				}

				PlatformHandler = virtualView.Handler as IPlatformViewHandler;
				InitializeContentConstraints(PlatformView);

				virtualView.BindingContext = bindingContext;
				itemsView.AddLogicalChild(virtualView);
			}

			if (PlatformHandler?.VirtualView is View view)
			{
				view.SetValueFromRenderer(BindableObject.BindingContextProperty, bindingContext);
			}
		}

		bool IsUsingVSMForSelectionColor(View view)
		{
			var groups = VisualStateManager.GetVisualStateGroups(view);
			for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
			{
				var group = groups[groupIndex];
				for (var stateIndex = 0; stateIndex < group.States.Count; stateIndex++)
				{
					var state = group.States[stateIndex];
					if (state.Name != VisualStateManager.CommonStates.Selected)
					{
						continue;
					}

					for (var setterIndex = 0; setterIndex < state.Setters.Count; setterIndex++)
					{
						var setter = state.Setters[setterIndex];
						if (setter.Property.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public override bool Selected
		{
			get => base.Selected;
			set
			{
				base.Selected = value;

				UpdateVisualStates();

				if (base.Selected)
				{
					// This must be called here otherwise the first item will have a gray background
					UpdateSelectionColor();
				}
			}
		}

		//protected abstract (bool, Size) NeedsContentSizeUpdate(Size currentSize);

		// void MeasureInvalidated(object sender, EventArgs args)
		// {
		// 	var (needsUpdate, toSize) = NeedsContentSizeUpdate(_size);
		//
		// 	if (!needsUpdate)
		// 	{
		// 		return;
		// 	}
		//
		// 	// Cache the size for next time
		// 	_size = toSize;
		//
		// 	// Let the controller know that things need to be laid out again
		// 	OnContentSizeChanged();
		// }

		protected void OnContentSizeChanged()
		{
			_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(ContentSizeChanged));
		}

		protected void OnLayoutAttributesChanged(UICollectionViewLayoutAttributes newAttributes)
		{
			_weakEventManager.HandleEvent(this, new LayoutAttributesChangedEventArgs2(newAttributes), nameof(LayoutAttributesChanged));
		}

		//protected abstract bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes);

		void UpdateVisualStates()
		{
			if (PlatformHandler?.VirtualView is VisualElement element)
			{
				VisualStateManager.GoToState(element, Selected
					? VisualStateManager.CommonStates.Selected
					: VisualStateManager.CommonStates.Normal);
			}
		}

		void UpdateSelectionColor()
		{
			if (PlatformHandler?.VirtualView is not View view)
			{
				return;
			}

			UpdateSelectionColor(view);
		}

		void UpdateSelectionColor(View view)
		{
			if (SelectedBackgroundView is null)
			{
				return;
			}

			// Prevents the use of default color when there are VisualStateManager with Selected state setting the background color
			// First we check whether the cell has the default selected background color; if it does, then we should check
			// to see if the cell content is the VSM to set a selected color

			if (ColorExtensions.AreEqual(SelectedBackgroundView.BackgroundColor, ColorExtensions.Gray) && IsUsingVSMForSelectionColor(view))
			{
				SelectedBackgroundView.BackgroundColor = UIColor.Clear;
			}
		}
	}
}
