#nullable disable
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ContentView.xml" path="Type[@FullName='Microsoft.Maui.Controls.ContentView']/Docs/*" />
	[ContentProperty("Content")]
	public partial class ContentView : TemplatedView
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ContentView.xml" path="//Member[@MemberName='ContentProperty']/Docs/*" />
		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(ContentView), null, propertyChanged: TemplateUtilities.OnContentChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/ContentView.xml" path="//Member[@MemberName='Content']/Docs/*" />
		public View Content
		{
			get { return (View)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			IView content = Content;

			if (content == null && (this as IContentView)?.PresentedContent is IView presentedContent)
				content = presentedContent;

			ControlTemplate controlTemplate = ControlTemplate;

			if (content is BindableObject bindableContent && controlTemplate != null)
				SetInheritedBindingContext(bindableContent, BindingContext);
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