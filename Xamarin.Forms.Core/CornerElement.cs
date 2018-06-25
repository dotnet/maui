namespace Xamarin.Forms
{
	static class CornerElement
	{
		public static readonly BindableProperty CornerRadiusProperty =
			BindableProperty.Create(nameof(CornerRadius), typeof(CornerRadius), typeof(ICornerElement), default(CornerRadius));
	}
}