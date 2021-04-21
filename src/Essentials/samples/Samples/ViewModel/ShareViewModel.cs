using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using Samples.Helpers;

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
		string shareFilesTitle;
		string shareFile1AttachmentContents;
		string shareFile1AttachmentName;
		string shareFile2AttachmentContents;
		string shareFile2AttachmentName;

		public ICommand RequestCommand { get; }

		public ICommand RequestFileCommand { get; }

		public ICommand RequestFilesCommand { get; }

		public ShareViewModel()
		{
			RequestCommand = new Command<Microsoft.Maui.Controls.View>(OnRequest);
			RequestFileCommand = new Command<Microsoft.Maui.Controls.View>(OnFileRequest);
			RequestFilesCommand = new Command<Microsoft.Maui.Controls.View>(OnFilesRequest);
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

		public string ShareFilesTitle
		{
			get => shareFilesTitle;
			set => SetProperty(ref shareFilesTitle, value);
		}

		public string ShareFile1AttachmentContents
		{
			get => shareFile1AttachmentContents;
			set => SetProperty(ref shareFile1AttachmentContents, value);
		}

		public string ShareFile1AttachmentName
		{
			get => shareFile1AttachmentName;
			set => SetProperty(ref shareFile1AttachmentName, value);
		}

		public string ShareFile2AttachmentContents
		{
			get => shareFile2AttachmentContents;
			set => SetProperty(ref shareFile2AttachmentContents, value);
		}

		public string ShareFile2AttachmentName
		{
			get => shareFile2AttachmentName;
			set => SetProperty(ref shareFile2AttachmentName, value);
		}

		async void OnRequest(Microsoft.Maui.Controls.View element)
			=> await Share.RequestAsync(new ShareTextRequest
			{
				Subject = Subject,
				Text = ShareText ? Text : null,
				Uri = ShareUri ? Uri : null,
				Title = Title,
				PresentationSourceBounds = element.GetAbsoluteBounds()
			});

		async void OnFileRequest(Microsoft.Maui.Controls.View element)
		{
			if (string.IsNullOrWhiteSpace(ShareFileAttachmentContents))
				return;

			var file = CreateFile(ShareFileAttachmentName, ShareFileAttachmentContents, "Attachment.txt");

			await Share.RequestAsync(new ShareFileRequest
			{
				Title = ShareFileTitle,
				File = new ShareFile(file),
				PresentationSourceBounds = element.GetAbsoluteBounds()
			});
		}

		async void OnFilesRequest(Microsoft.Maui.Controls.View element)
		{
			if (string.IsNullOrWhiteSpace(ShareFile1AttachmentContents) ||
				string.IsNullOrWhiteSpace(ShareFile2AttachmentContents))
				return;

			var file1 = CreateFile(ShareFile1AttachmentName, ShareFile1AttachmentContents, "Attachment1.txt");
			var file2 = CreateFile(ShareFile2AttachmentName, ShareFile2AttachmentContents, "Attachment2.txt");

			await Share.RequestAsync(new ShareMultipleFilesRequest
			{
				Title = ShareFilesTitle,
				Files = new List<ShareFile> { new ShareFile(file1), new ShareFile(file2) },
				PresentationSourceBounds = element.GetAbsoluteBounds()
			});
		}

		string CreateFile(string fileName, string fileContents, string emptyName)
		{
			var fn = string.IsNullOrWhiteSpace(fileName) ? emptyName : fileName.Trim();
			var file = Path.Combine(FileSystem.CacheDirectory, fn);
			File.WriteAllText(file, fileContents);
			return file;
		}
	}
}
