namespace Xamarin.Forms.Pages
{
	public class DirectoryPage : DataPage
	{
		public static readonly BindableProperty IsGroupingEnabledProperty = BindableProperty.Create(nameof(IsGroupingEnabled), typeof(bool), typeof(DirectoryPage), default(bool));

		public bool IsGroupingEnabled
		{
			get { return (bool)GetValue(IsGroupingEnabledProperty); }
			set { SetValue(IsGroupingEnabledProperty, value); }
		}
	}
}