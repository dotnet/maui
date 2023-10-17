using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class MenuBarViewModel : BaseViewModel
	{
	

		private string icon1;
		private string icon2;

		
		public string Icon1
		{
			get { return icon1; }
			set
			{
				SetProperty(ref icon1, value);
			}
		}

		public string Icon2
		{
			get { return icon2; }
			set
			{
				SetProperty(ref icon2, value);
			}
		}

		

		public MenuBarViewModel()
		{
			Icon1 = "\u26f9";
			Icon2 = "\u26fa";
		}

		public void SwapIcon()
		{
			Icon1 = "\u26fa";
			Icon2 = "\u26f9";
		}
	}
}