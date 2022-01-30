namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ContentPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.ContentPage']/Docs" />
	[ContentProperty("Content")]
	public partial class ContentPage : TemplatedPage
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ContentPage.xml" path="//Member[@MemberName='ContentProperty']/Docs" />
		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(ContentPage), null, propertyChanged: TemplateUtilities.OnContentChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/ContentPage.xml" path="//Member[@MemberName='Content']/Docs" />
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