#nullable disable
namespace Microsoft.Maui.Controls
{
	static class CornerElement
	{
		/// <summary>Bindable property for <see cref="CornerRadius"/>.</summary>
		public static readonly BindableProperty CornerRadiusProperty =
			BindableProperty.Create(nameof(CornerRadius), typeof(CornerRadius), typeof(ICornerElement), default(CornerRadius));
	}
}