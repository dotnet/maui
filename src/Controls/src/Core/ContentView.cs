using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	[ContentProperty("Content")]
	public class ContentView : TemplatedView, IContentView
	{
		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(ContentView), null, propertyChanged: TemplateUtilities.OnContentChanged);

		public View Content
		{
			get { return (View)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		object IContentView.Content => Content;
		IView IContentView.PresentedContent => ((this as IControlTemplated).TemplateRoot as IView) ?? Content;

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			View content = Content;
			ControlTemplate controlTemplate = ControlTemplate;
			if (content != null && controlTemplate != null)
			{
				SetInheritedBindingContext(content, BindingContext);
			}
		}

		internal override void OnControlTemplateChanged(ControlTemplate oldValue, ControlTemplate newValue)
		{
			if (oldValue == null)
				return;

			base.OnControlTemplateChanged(oldValue, newValue);
			View content = Content;
			ControlTemplate controlTemplate = ControlTemplate;
			if (content != null && controlTemplate != null)
			{
				SetInheritedBindingContext(content, BindingContext);
			}
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			DesiredSize = this.ComputeDesiredSize(widthConstraint, heightConstraint);
			return DesiredSize;
		}

		Size IContentView.CrossPlatformMeasure(double widthConstraint, double heightConstraint) 
		{
			return this.MeasureContent(widthConstraint, heightConstraint);
		}

		protected override Size ArrangeOverride(Rectangle bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.NativeArrange(Frame);
			return Frame.Size;
		}

		Size IContentView.CrossPlatformArrange(Rectangle bounds) 
		{
			this.ArrangeContent(bounds);
			return bounds.Size;
		}
	}
}