using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Internals;
using WThickness = Windows.UI.Xaml.Thickness;
using WSize = Windows.Foundation.Size;

namespace Xamarin.Forms.Platform.UWP
{
	public class ItemContentControl : ContentControl
	{
		IVisualElementRenderer _renderer;

		public ItemContentControl()
		{
			DefaultStyleKey = typeof(ItemContentControl);
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

			if (oldContent is FrameworkElement oldElement)
			{
				oldElement.Loaded -= ContentLoaded;
			}

			if (newContent is FrameworkElement newElement)
			{
				newElement.Loaded += ContentLoaded;
			}
		}

		internal void Realize()
		{
			var dataContext = FormsDataContext;
			var formsTemplate = FormsDataTemplate;
			var container = FormsContainer;

			var itemsView = container as ItemsView;

			if (itemsView != null && _renderer?.Element != null)
			{
				itemsView.RemoveLogicalChild(_renderer.Element);
			}

			if (dataContext == null || formsTemplate == null || container == null)
			{
				return;
			}

			var view = FormsDataTemplate.CreateContent(dataContext, container) as View;
			view.BindingContext = dataContext;
			_renderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, _renderer);

			Content = _renderer.ContainerElement;

			itemsView?.AddLogicalChild(view);
		}

		void ContentLoaded(object sender, RoutedEventArgs e)
		{
			InvalidateMeasure();
		}

		protected override WSize MeasureOverride(WSize availableSize)
		{
			if (_renderer == null)
			{
				return base.MeasureOverride(availableSize);
			}

			var frameworkElement = Content as FrameworkElement;

			var formsElement = _renderer.Element;
			if (ItemHeight != default || ItemWidth != default)
			{
				formsElement.Layout(new Rectangle(0, 0, ItemWidth, ItemHeight));

				var wsize = new WSize(ItemWidth, ItemHeight);

				frameworkElement.Margin = new WThickness(ItemSpacing.Left, ItemSpacing.Top, ItemSpacing.Right, ItemSpacing.Bottom);

				frameworkElement.Measure(wsize);

				return base.MeasureOverride(wsize);
			}
			else
			{
				var (width, height) = formsElement.Measure(availableSize.Width, availableSize.Height,
					MeasureFlags.IncludeMargins).Request;

				width = Max(width, availableSize.Width);
				height = Max(height, availableSize.Height);

				formsElement.Layout(new Rectangle(0, 0, width, height));

				var wsize = new WSize(width, height);

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
	}
}