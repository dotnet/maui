#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WSize = Windows.Foundation.Size;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Controls.Platform
{
	public class ItemContentControl : ContentControl
	{
		VisualElement _visualElement;
		IViewHandler _handler;
		DataTemplate _currentTemplate;

		public ItemContentControl()
		{
			DefaultStyleKey = typeof(ItemContentControl);
			IsTabStop = false;
		}

		public static readonly DependencyProperty MauiContextProperty = DependencyProperty.Register(
			nameof(MauiContext), typeof(IMauiContext), typeof(ItemContentControl),
			new PropertyMetadata(default(IMauiContext), MauiContextChanged));

		static void MauiContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue == null)
			{
				return;
			}

			var itemContentControl = (ItemContentControl)d;
			itemContentControl.Realize();
		}

		public IMauiContext MauiContext
		{
			get => (IMauiContext)GetValue(MauiContextProperty);
			set => SetValue(MauiContextProperty, value);
		}

		public static readonly DependencyProperty FormsDataTemplateProperty = DependencyProperty.Register(
			nameof(FormsDataTemplate), typeof(DataTemplate), typeof(ItemContentControl),
			new PropertyMetadata(default(DataTemplate), FormsDataTemplateChanged));

		static void FormsDataTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue == null)
			{
				return;
			}

			var itemContentControl = (ItemContentControl)d;
			itemContentControl.Realize();
		}

		public DataTemplate FormsDataTemplate
		{
			get => (DataTemplate)GetValue(FormsDataTemplateProperty);
			set => SetValue(FormsDataTemplateProperty, value);
		}

		public static readonly DependencyProperty FormsDataContextProperty = DependencyProperty.Register(
			nameof(FormsDataContext), typeof(object), typeof(ItemContentControl),
			new PropertyMetadata(default(object), FormsDataContextChanged));

		static void FormsDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var formsContentControl = (ItemContentControl)d;
			formsContentControl.Realize();
		}

		public object FormsDataContext
		{
			get => GetValue(FormsDataContextProperty);
			set => SetValue(FormsDataContextProperty, value);
		}

		public static readonly DependencyProperty FormsContainerProperty = DependencyProperty.Register(
			nameof(FormsContainer), typeof(BindableObject), typeof(ItemContentControl),
			new PropertyMetadata(default(BindableObject), FormsContainerChanged));

		static void FormsContainerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var formsContentControl = (ItemContentControl)d;
			formsContentControl.Realize();
		}

		public BindableObject FormsContainer
		{
			get => (BindableObject)GetValue(FormsContainerProperty);
			set => SetValue(FormsContainerProperty, value);
		}

		public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
			nameof(ItemHeight), typeof(double), typeof(ItemContentControl),
			new PropertyMetadata(default(double)));

		public double ItemHeight
		{
			get => (double)GetValue(ItemHeightProperty);
			set => SetValue(ItemHeightProperty, value);
		}

		public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
			nameof(ItemWidth), typeof(double), typeof(ItemContentControl),
			new PropertyMetadata(default(double)));

		public double ItemWidth
		{
			get => (double)GetValue(ItemWidthProperty);
			set => SetValue(ItemWidthProperty, value);
		}

		public static readonly DependencyProperty ItemSpacingProperty = DependencyProperty.Register(
			nameof(ItemSpacing), typeof(Thickness), typeof(ItemContentControl),
			new PropertyMetadata(default(Thickness)));

		public Thickness ItemSpacing
		{
			get => (Thickness)GetValue(ItemSpacingProperty);
			set => SetValue(ItemSpacingProperty, value);
		}

		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);

			if (oldContent != null && _visualElement != null)
			{
				_visualElement.MeasureInvalidated -= OnViewMeasureInvalidated;
				_visualElement.PropertyChanged -= OnViewPropertyChanged;
			}

			if (newContent != null && _visualElement != null)
			{
				_visualElement.MeasureInvalidated += OnViewMeasureInvalidated;
				_visualElement.PropertyChanged += OnViewPropertyChanged;
				UpdateSemanticProperties(_visualElement);
			}
		}

		internal void Realize()
		{
			var dataContext = FormsDataContext;
			var formsTemplate = FormsDataTemplate;
			var container = FormsContainer;
			var mauiContext = MauiContext;

			var itemsView = container as ItemsView;

			if (itemsView != null && _handler?.VirtualView is Element e)
			{
				itemsView.RemoveLogicalChild(e);
			}

			if (dataContext is null || formsTemplate is null || container is null || mauiContext is null)
			{
				return;
			}

			if (Content is null || _currentTemplate != formsTemplate)
			{
				// If the content has never been realized (i.e., this is a new instance), 
				// or if we need to switch DataTemplates (because this instance is being recycled)
				// then we'll need to create the content from the template 
				_visualElement = formsTemplate.CreateContent(dataContext, container) as VisualElement;
				_visualElement.BindingContext = dataContext;
				_handler = _visualElement.ToHandler(mauiContext);

				// We need to set IsPlatformStateConsistent explicitly; otherwise, it won't be set until the renderer's Loaded 
				// event. If the CollectionView is in a Layout, the Layout won't measure or layout the CollectionView until
				// every visible descendant has `IsPlatformStateConsistent == true`. And the problem that Layout is trying
				// to avoid by skipping layout for controls with not-yet-loaded children does not apply to CollectionView
				// items. If we don't set this, the CollectionView just won't get layout at all, and will be invisible until
				// the window is resized. 
				SetNativeStateConsistent(_visualElement);

				// Keep track of the template in case this instance gets reused later
				_currentTemplate = formsTemplate;
			}
			else
			{
				// We are reusing this ItemContentControl and we can reuse the Element
				_visualElement.BindingContext = dataContext;
			}

			if (_handler.VirtualView is ICrossPlatformLayout)
			{
				Content = _handler.ToPlatform();
			}
			else
			{
				Content = new ContentLayoutPanel(_handler.VirtualView);
			}

			itemsView?.AddLogicalChild(_visualElement);
		}

		void SetNativeStateConsistent(VisualElement visualElement)
		{
			visualElement.IsPlatformStateConsistent = true;

			foreach (var child in ((IElementController)visualElement).LogicalChildren)
			{
				if (!(child is VisualElement ve))
				{
					continue;
				}

				SetNativeStateConsistent(ve);
			}
		}

		internal void UpdateIsSelected(bool isSelected)
		{
			var formsElement = _handler?.VirtualView as VisualElement;

			if (formsElement == null)
				return;

			VisualStateManager.GoToState(formsElement, isSelected
				? VisualStateManager.CommonStates.Selected
				: VisualStateManager.CommonStates.Normal);
		}

		void OnViewMeasureInvalidated(object sender, EventArgs e)
		{
			InvalidateMeasure();
		}

		void OnViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.IsOneOf(
				SemanticProperties.HeadingLevelProperty,
				SemanticProperties.HintProperty,
				SemanticProperties.DescriptionProperty,
				AutomationProperties.IsInAccessibleTreeProperty) &&
				sender is IView view)
			{
				UpdateSemanticProperties(view);
			}
		}

		void UpdateSemanticProperties(IView view)
		{
			// If you don't set the automation properties on the root element
			// of a list item it just reads out the class type to narrator
			// https://docs.microsoft.com/en-us/accessibility-tools-docs/items/uwpxaml/listitem_name
			// Because this is the root element of the ListViewItem we need to propagate
			// the semantic properties from the root xplat element to this platform element
			if (view == null)
				return;

			this.UpdateSemantics(view);

			var semantics = view.Semantics;

			UI.Xaml.Automation.Peers.AccessibilityView defaultAccessibilityView =
				UI.Xaml.Automation.Peers.AccessibilityView.Content;

			if (!String.IsNullOrWhiteSpace(semantics?.Description) || !String.IsNullOrWhiteSpace(semantics?.Hint))
			{
				defaultAccessibilityView = UI.Xaml.Automation.Peers.AccessibilityView.Raw;
			}

			this.SetAutomationPropertiesAccessibilityView(_visualElement, defaultAccessibilityView);
		}

		/// <inheritdoc/>
		protected override WSize ArrangeOverride(WSize finalSize)
		{
			return base.ArrangeOverride(finalSize);
		}

		/// <inheritdoc/>
		protected override WSize MeasureOverride(WSize availableSize)
		{
			if (_handler is null)
			{
				// Make sure we supply a real number for sizes otherwise virtualization won't function
				if (double.IsFinite(availableSize.Width) && !double.IsFinite(availableSize.Height))
					return new WSize(availableSize.Width, 32);
				else if (!double.IsFinite(availableSize.Width) && double.IsFinite(availableSize.Height))
					return new WSize(88, availableSize.Height);

				return base.MeasureOverride(availableSize);
			}

			var width = ItemWidth == default ? availableSize.Width : ItemWidth;
			var height = ItemHeight == default ? availableSize.Height : ItemHeight;

			// I realize if ItemWidth and ItemHeight are set that this call seems pointless, but it's not.
			// From what I can tell, calling `base.MeasureOverride` causes the `ContentControl` to realize its content
			// and build its visual tree. So, in order to just play nice with the WinUI `ContentControl` we always
			// call base.MeasureOverride, so that we don't short circuit whatever bookkeeping it needs to do.

			var measureSize = base.MeasureOverride(new WSize(width, height));

			return new WSize(Max(measureSize.Width, width), Max(measureSize.Height, height));
		}

		double Max(double requested, double available)
		{
			return Math.Max(requested, ClampInfinity(available));
		}

		double ClampInfinity(double value)
		{
			return double.IsInfinity(value) ? 0 : value;
		}

		internal VisualElement GetVisualElement() => _visualElement;

		class ContentLayoutPanel : Panel
		{
			IView _view;
			public ContentLayoutPanel(IView view)
			{
				_view = view;

				var platformView = view.ToPlatform();

				// Just in case this view is already parented to a wrapper that's been cycled out
				if (platformView.Parent is ContentLayoutPanel clp)
					clp.Children.Remove(platformView);

				Children.Add(platformView);
			}

			protected override WSize ArrangeOverride(WSize finalSize) => _view.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height)).ToPlatform();

			protected override WSize MeasureOverride(WSize availableSize) => _view.Measure(availableSize.Width, availableSize.Height).ToPlatform();
		}
	}
}