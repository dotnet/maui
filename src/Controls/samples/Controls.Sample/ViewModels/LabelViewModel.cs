using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class LabelViewModel : BaseViewModel
	{
		string _htmlString = "Html, &lt;b&gt;from ViewModel!&lt;/b&gt;";

		public string HtmlString
		{
			get { return _htmlString; }
			set
			{
				_htmlString = value;
				OnPropertyChanged();
			}
		}
	}
}