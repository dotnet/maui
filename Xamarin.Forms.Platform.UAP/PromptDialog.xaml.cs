using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Xamarin.Forms.Platform.UWP
{
	public sealed partial class PromptDialog : ContentDialog
	{
		public PromptDialog()
		{
			this.InitializeComponent();
		}

		public string Message
		{
			get => TextBlockMessage.Text;
			set => TextBlockMessage.Text = value;
		}

		public string Input
		{
			get => TextBoxInput.Text;
			set => TextBoxInput.Text = value;
		}

		public string Placeholder
		{
			get => TextBoxInput.PlaceholderText;
			set => TextBoxInput.PlaceholderText = value;
		}

		public int MaxLength
		{
			get => TextBoxInput.MaxLength;
			set => TextBoxInput.MaxLength = value;
		}

		public InputScope InputScope
		{
			get => TextBoxInput.InputScope;
			set => TextBoxInput.InputScope = value;
		}
	}
}
