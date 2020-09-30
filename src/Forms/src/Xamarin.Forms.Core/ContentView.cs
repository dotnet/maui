namespace Xamarin.Forms
{
	[ContentProperty("Content")]
	public class ContentView : TemplatedView
	{
		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(ContentView), null, propertyChanged: TemplateUtilities.OnContentChanged);

		public View Content
		{
			get { return (View)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

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
	}
}