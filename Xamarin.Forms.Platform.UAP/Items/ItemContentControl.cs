using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	public class ItemContentControl : UserControl
	{
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

			var formsContentControl = (ItemContentControl)d;
			formsContentControl.RealizeFormsDataTemplate((DataTemplate)e.NewValue);
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
			formsContentControl.SetFormsDataContext(e.NewValue);
		}

		public object FormsDataContext
		{
			get => GetValue(FormsDataContextProperty);
			set => SetValue(FormsDataContextProperty, value);
		}

		VisualElement _rootElement;

		internal void RealizeFormsDataTemplate(DataTemplate template)
		{
			var content = FormsDataTemplate.CreateContent();

			if (content is VisualElement visualElement)
			{
				if (_rootElement != null)
				{
					_rootElement.MeasureInvalidated -= RootElementOnMeasureInvalidated;
				}

				_rootElement = visualElement;
				_rootElement.MeasureInvalidated += RootElementOnMeasureInvalidated;

				Content = Platform.CreateRenderer(visualElement).ContainerElement;
			}

			if (FormsDataContext != null)
			{
				SetFormsDataContext(FormsDataContext);
			}
		}

		void RootElementOnMeasureInvalidated(object sender, EventArgs e)
		{
			InvalidateMeasure();
		}

		internal void SetFormsDataContext(object context)
		{
			if (_rootElement == null)
			{
				return;
			}

			BindableObject.SetInheritedBindingContext(_rootElement, context);
		}

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			if (_rootElement == null)
			{
				return base.MeasureOverride(availableSize);
			}

			Size request = _rootElement.Measure(availableSize.Width, availableSize.Height, 
				MeasureFlags.IncludeMargins).Request;

			_rootElement.Layout(new Rectangle(Point.Zero, request));

			return new Windows.Foundation.Size(request.Width, request.Height); 
		}

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			if (!(Content is FrameworkElement frameworkElement))
			{
				return finalSize;
			}
		
			frameworkElement.Arrange(new Rect(_rootElement.X, _rootElement.Y, _rootElement.Width, _rootElement.Height));
			return base.ArrangeOverride(finalSize);
		}
	}
}