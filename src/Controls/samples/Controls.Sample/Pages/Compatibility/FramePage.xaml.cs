namespace Maui.Controls.Sample.Pages
{
	public partial class FramePage
	{
		public FramePage()
		{
			InitializeComponent();
		}

		void OnHasShadowButtonClicked(object sender, System.EventArgs e)
		{
			HasShadowFrame.HasShadow = !HasShadowFrame.HasShadow;
		}
	}
}