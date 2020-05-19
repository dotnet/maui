namespace System.Maui
{
	static class BarElement
	{
		public static readonly BindableProperty BarBackgroundColorProperty =
			BindableProperty.Create(nameof(IBarElement.BarBackgroundColor), typeof(Color), typeof(IBarElement), default(Color));

		public static readonly BindableProperty BarTextColorProperty =
			BindableProperty.Create(nameof(IBarElement.BarTextColor), typeof(Color), typeof(IBarElement), default(Color));
	}
}
