using System.Windows.Input;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    class ClipboardViewModel : BaseViewModel
    {
        string fieldValue;

        public ClipboardViewModel()
        {
            CopyCommand = new Command(OnCopy);
            PasteCommand = new Command(OnPaste);
        }

        public ICommand CopyCommand { get; }

        public ICommand PasteCommand { get; }

        async void OnCopy() => await Clipboard.SetTextAsync(FieldValue);

        async void OnPaste()
        {
            var text = await Clipboard.GetTextAsync();
            if (!string.IsNullOrWhiteSpace(text))
            {
                FieldValue = text;
            }
        }

        public string FieldValue
        {
            get => fieldValue;
            set => SetProperty(ref fieldValue, value);
        }
    }
}
