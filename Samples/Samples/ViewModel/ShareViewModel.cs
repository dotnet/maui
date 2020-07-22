using System.IO;
using System.Windows.Input;
using Samples.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    class ShareViewModel : BaseViewModel
    {
        bool shareText = true;
        bool shareUri;
        string text;
        string uri;
        string subject;
        string title;
        string shareFileAttachmentContents;
        string shareFileAttachmentName;
        string shareFileTitle;

        public ICommand RequestCommand { get; }

        public ICommand RequestFileCommand { get; }

        public ShareViewModel()
        {
            RequestCommand = new Command<Xamarin.Forms.View>(OnRequest);
            RequestFileCommand = new Command<Xamarin.Forms.View>(OnFileRequest);
        }

        public bool ShareText
        {
            get => shareText;
            set => SetProperty(ref shareText, value);
        }

        public bool ShareUri
        {
            get => shareUri;
            set => SetProperty(ref shareUri, value);
        }

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        public string Uri
        {
            get => uri;
            set => SetProperty(ref uri, value);
        }

        public string Subject
        {
            get => subject;
            set => SetProperty(ref subject, value);
        }

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public string ShareFileTitle
        {
            get => shareFileTitle;
            set => SetProperty(ref shareFileTitle, value);
        }

        public string ShareFileAttachmentContents
        {
            get => shareFileAttachmentContents;
            set => SetProperty(ref shareFileAttachmentContents, value);
        }

        public string ShareFileAttachmentName
        {
            get => shareFileAttachmentName;
            set => SetProperty(ref shareFileAttachmentName, value);
        }

        async void OnRequest(Xamarin.Forms.View element)
        {
            var bounds = element.GetAbsoluteBounds();

            await Share.RequestAsync(new ShareTextRequest
            {
                Subject = Subject,
                Text = ShareText ? Text : null,
                Uri = ShareUri ? Uri : null,
                Title = Title,
                PresentationSourceBounds = bounds.ToSystemRectangle()
            });
        }

        async void OnFileRequest(Xamarin.Forms.View element)
        {
            if (string.IsNullOrWhiteSpace(ShareFileAttachmentContents))
                return;

            // create a temprary file
            var fn = string.IsNullOrWhiteSpace(ShareFileAttachmentName)
                ? "Attachment.txt"
                : ShareFileAttachmentName.Trim();
            var file = Path.Combine(FileSystem.CacheDirectory, fn);
            File.WriteAllText(file, ShareFileAttachmentContents);

            var bounds = element.GetAbsoluteBounds();

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = Title,
                File = new ShareFile(file),
                PresentationSourceBounds = bounds.ToSystemRectangle()
            });
        }
    }
}
