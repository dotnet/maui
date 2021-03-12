namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public class BaseView : ContentPage
	{
		public BaseView()
		{
			SetBinding(Page.TitleProperty, new Binding(HBaseViewModel.TitlePropertyName));
			SetBinding(Page.IconImageSourceProperty, new Binding(HBaseViewModel.IconPropertyName));
		}
	}

	public class HanselmanNavigationPage : NavigationPage
	{
		public HanselmanNavigationPage(Page root) : base(root)
		{
			Init();
		}

		public HanselmanNavigationPage()
		{
			Init();
		}

		void Init()
		{

			BarBackgroundColor = Color.FromHex("#03A9F4");
			BarTextColor = Color.White;
		}
	}

}