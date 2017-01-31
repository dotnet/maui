
namespace Xamarin.Forms
{
	public class Accessibility
	{
		public static readonly BindableProperty HintProperty = BindableProperty.Create("Hint", typeof(string), typeof(Element), default(string));

		public static readonly BindableProperty IsInAccessibleTreeProperty = BindableProperty.Create("IsInAccessibleTree", typeof(bool?), typeof(Element), null);

		public static readonly BindableProperty LabeledByProperty = BindableProperty.Create("LabeledBy", typeof(VisualElement), typeof(Element), default(VisualElement));

		public static readonly BindableProperty NameProperty = BindableProperty.Create("Name", typeof(string), typeof(Element), default(string));
	}
}
