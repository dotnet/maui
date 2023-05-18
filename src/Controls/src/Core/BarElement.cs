#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	static class BarElement
	{
		/// <summary>Bindable property for <see cref="IBarElement.BarBackgroundColor"/>.</summary>
		public static readonly BindableProperty BarBackgroundColorProperty =
			BindableProperty.Create(nameof(IBarElement.BarBackgroundColor), typeof(Color), typeof(IBarElement), default(Color));

		/// <summary>Bindable property for <see cref="IBarElement.BarBackground"/>.</summary>
		public static readonly BindableProperty BarBackgroundProperty =
			BindableProperty.Create(nameof(IBarElement.BarBackground), typeof(Brush), typeof(IBarElement), default(Brush));

		/// <summary>Bindable property for <see cref="IBarElement.BarTextColor"/>.</summary>
		public static readonly BindableProperty BarTextColorProperty =
			BindableProperty.Create(nameof(IBarElement.BarTextColor), typeof(Color), typeof(IBarElement), default(Color));
	}
}