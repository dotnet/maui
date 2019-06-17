using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	public class ItemContentControl : ContentControl
	{
		IVisualElementRenderer _renderer;

		public ItemContentControl()
		{
			CollectionView.VerifyCollectionViewFlagEnabled(nameof(ItemContentControl));
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

			if (dataContext == null || formsTemplate == null)
			{
				return;
			}

			// TODO ezhart Handle SelectDataTemplate

			var view = FormsDataTemplate.CreateContent() as View;

			_renderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, _renderer);

			Content = _renderer.ContainerElement;

			// TODO ezhart Add View as a logical child of the ItemsView
			
			BindableObject.SetInheritedBindingContext(_renderer.Element, dataContext);
		}

		void ContentLoaded(object sender, RoutedEventArgs e)
		{
			InvalidateMeasure();
		}

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			if (_renderer == null)
			{
				return base.MeasureOverride(availableSize);
			}

			var formsElement = _renderer.Element;

			Size request = formsElement.Measure(availableSize.Width, availableSize.Height,
				MeasureFlags.IncludeMargins).Request;

			formsElement.Layout(new Rectangle(0, 0, request.Width, request.Height));

			var wsize = new Windows.Foundation.Size(request.Width, request.Height);

			(Content as FrameworkElement).Measure(wsize);

			return base.MeasureOverride(wsize);
		}
	}
}