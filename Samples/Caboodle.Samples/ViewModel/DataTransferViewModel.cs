using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Microsoft.Caboodle;
using Xamarin.Forms;

namespace Caboodle.Samples.ViewModel
{
    class DataTransferViewModel : BaseViewModel
    {
        public ICommand RequestCommand { get; }

        public DataTransferViewModel()
        {
            RequestCommand = new Command(async () =>
            {
                await DataTransfer.RequestAsync(new ShareTextRequest
                {
                    Subject = Subject,
                    Text = ShareText ? Text : null,
                    Uri = ShareUri ? Uri : null,
                    Title = Title
                });
            });
        }

        bool shareText = true;

        public bool ShareText
        {
            get => shareText;
            set => SetProperty(ref shareText, value);
        }

        bool shareUri;

        public bool ShareUri
        {
            get => shareUri;
            set => SetProperty(ref shareUri, value);
        }

        string text;

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        string uri;

        public string Uri
        {
            get => uri;
            set => SetProperty(ref uri, value);
        }

        string subject;

        public string Subject
        {
            get => subject;
            set => SetProperty(ref subject, value);
        }

        string title;

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }
    }
}
