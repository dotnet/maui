using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class ContentViewPage
	{
		public ContentViewPage()
		{
			InitializeComponent();

			BindingContext = new ContentViewModel();
		}
	}

	public class ContentViewModel : BindableObject
	{
		private string _text;

		public ContentViewModel()
		{
			_text = "Content";
		}

		public string Text
		{
			get => _text;
			set
			{
				if (_text != value)
				{
					_text = value;
					OnPropertyChanged();
				}
			}
		}
	}
}