using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Controls.Internals;
using WThickness = Microsoft.UI.Xaml.Thickness;
using WSize = Windows.Foundation.Size;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Platform
{
	public class ItemContentControl : ContentControl
	{
		VisualElement _visualElement;
		IViewHandler _renderer;
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

			if (itemsView != null && _renderer?.VirtualView is Element e)
			{
				itemsView.RemoveLogicalChild(e);
			}

			if (dataContext == null || formsTemplate == null || container == null || mauiContext == null)
			{
				return;
			}

			if (_renderer?.ContainerView == null || _currentTemplate != formsTemplate)
			{
				// If the content has never been realized (i.e., this is a new instance), 
				// or if we need to switch DataTemplates (because this instance is being recycled)
				// then we'll need to create the content from the template 
				_visualElement = formsTemplate.CreateContent(dataContext, container) as VisualElement;
				_visualElement.BindingContext = dataContext;
				_renderer = _visualElement.ToHandler(mauiContext);

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
				_visualElement = _renderer.VirtualView as VisualElement;
				_visualElement.BindingContext = dataContext;
			}

			Content = _renderer.ToPlatform();
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
			var formsElement = _renderer?.VirtualView as VisualElement;

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

		protected override WSize MeasureOverride(WSize availableSize)
		{
			if (_renderer == null)
			{
				return base.MeasureOverride(availableSize);
			}

			var frameworkElement = Content as FrameworkElement;
			var formsElement = _renderer.VirtualView as VisualElement;
			var margin = _renderer.VirtualView.Margin;

			if (ItemHeight != default || ItemWidth != default)
			{
				formsElement.Layout(new Rect(0, 0, ItemWidth, ItemHeight));

				var wsize = new WSize(ItemWidth, ItemHeight);

				frameworkElement.Margin =
					WinUIHelpers.CreateThickness(
						margin.Left + ItemSpacing.Left,
						margin.Top + ItemSpacing.Top,
						margin.Right + ItemSpacing.Right,
						margin.Bottom + ItemSpacing.Bottom);

				if (CanMeasureContent(frameworkElement))
					frameworkElement.Measure(wsize);

				return base.MeasureOverride(wsize);
			}
			else
			{
				var (width, height) = formsElement.Measure(availableSize.Width, availableSize.Height,
					MeasureFlags.IncludeMargins).Request;

				width = Max(width, availableSize.Width);
				height = Max(height, availableSize.Height);

				formsElement.Layout(new Rect(0, 0, width, height));

				var wsize = new WSize(width, height);

				frameworkElement.Margin = WinUIHelpers.CreateThickness(
					margin.Left,
					margin.Top,
					margin.Right,
					margin.Bottom);

				if (CanMeasureContent(frameworkElement))
					frameworkElement.Measure(wsize);

				return base.MeasureOverride(wsize);
			}
		}

		double Max(double requested, double available)
		{
			return Math.Max(requested, ClampInfinity(available));
		}

		double ClampInfinity(double value)
		{
			return double.IsInfinity(value) ? 0 : value;
		}

		bool CanMeasureContent(FrameworkElement frameworkElement)
		{
			// Measure the SwipeControl before has loaded causes a crash on the first layout pass
			if (frameworkElement is SwipeControl swipeControl && !swipeControl.IsLoaded)
				return false;

			return true;
		}
	}
}