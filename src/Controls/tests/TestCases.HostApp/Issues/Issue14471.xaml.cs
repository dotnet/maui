﻿namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 14471, "Image can disappear when going back to the page", PlatformAffected.Android)]
	public partial class Issue14471 : Shell
	{
		public Issue14471()
		{
			InitializeComponent();
		}
	}

	public class Issue14471Tab1Content : ContentPage
	{

		public Issue14471Tab1Content()
		{
			var image = new Image() { HeightRequest = 100, AutomationId = "image" };
			var imageButton = new ImageButton() { HeightRequest = 100, AutomationId = "imageButton" };
			Content = new VerticalStackLayout()
			{
				new Button()
				{
					AutomationId = "switchToTab2Button",
					Text = "Switch to tab 2",
					Command = new Command(async () => await Shell.Current.GoToAsync("//tab2"))
				},
				image,
				imageButton
			};
			LoadImage((source) => image.Source = source);
			LoadImage((source) => imageButton.Source = source);
		}

		async void LoadImage(Action<ImageSource> setImageSource)
		{
			await Task.Delay(1);
			var imageBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAB+FBMVEUAAAA/mUPidDHiLi5Cn0XkNTPmeUrkdUg/m0Q0pEfcpSbwaVdKskg+lUP4zA/iLi3msSHkOjVAmETdJSjtYFE/lkPnRj3sWUs8kkLeqCVIq0fxvhXqUkbVmSjwa1n1yBLepyX1xxP0xRXqUkboST9KukpHpUbuvRrzrhF/ljbwaljuZFM4jELaoSdLtElJrUj1xxP6zwzfqSU4i0HYnydMtUlIqUfywxb60AxZqEXaoifgMCXptR9MtklHpEY2iUHWnSjvvRr70QujkC+pUC/90glMuEnlOjVMt0j70QriLS1LtEnnRj3qUUXfIidOjsxAhcZFo0bjNDH0xxNLr0dIrUdmntVTkMoyfL8jcLBRuErhJyrgKyb4zA/5zg3tYFBBmUTmQTnhMinruBzvvhnxwxZ/st+Ktt5zp9hqota2vtK6y9FemNBblc9HiMiTtMbFtsM6gcPV2r6dwroseLrMrbQrdLGdyKoobKbo3Zh+ynrgVllZulTsXE3rV0pIqUf42UVUo0JyjEHoS0HmsiHRGR/lmRz/1hjqnxjvpRWfwtOhusaz0LRGf7FEfbDVmqHXlJeW0pbXq5bec3fX0nTnzmuJuWvhoFFhm0FtrziBsjaAaDCYWC+uSi6jQS3FsSfLJiTirCOkuCG1KiG+wSC+GBvgyhTszQ64Z77KAAAARXRSTlMAIQRDLyUgCwsE6ebm5ubg2dLR0byXl4FDQzU1NDEuLSUgC+vr6urq6ubb29vb2tra2tG8vLu7u7uXl5eXgYGBgYGBLiUALabIAAABsElEQVQoz12S9VPjQBxHt8VaOA6HE+AOzv1wd7pJk5I2adpCC7RUcHd3d3fXf5PvLkxheD++z+yb7GSRlwD/+Hj/APQCZWxM5M+goF+RMbHK594v+tPoiN1uHxkt+xzt9+R9wnRTZZQpXQ0T5uP1IQxToyOAZiQu5HEpjeA4SWIoksRxNiGC1tRZJ4LNxgHgnU5nJZBDvuDdl8lzQRBsQ+s9PZt7s7Pz8wsL39/DkIfZ4xlB2Gqsq62ta9oxVlVrNZpihFRpGO9fzQw1ms0NDWZz07iGkJmIFH8xxkc3a/WWlubmFkv9AB2SEpDvKxbjidN2faseaNV3zoHXvv7wMODJdkOHAegweAfFPx4G67KluxzottCU9n8CUqXzcIQdXOytAHqXxomvykhEKN9EFutG22p//0rbNvHVxiJywa8yS2KDfV1dfbu31H8jF1RHiTKtWYeHxUvq3bn0pyjCRaiRU6aDO+gb3aEfEeVNsDgm8zzLy9egPa7Qt8TSJdwhjplk06HH43ZNJ3s91KKCHQ5x4sw1fRGYDZ0n1L4FKb9/BP5JLYxToheoFCVxz57PPS8UhhEpLBVeAAAAAElFTkSuQmCC");
			setImageSource.Invoke(ImageSource.FromStream(() => new MemoryStream(imageBytes)));
		}
	}

	public class Issue14471Tab2Content : ContentPage
	{
		public Issue14471Tab2Content()
		{
			Content = new Button()
			{
				AutomationId = "switchToTab1Button",
				Text = "Switch to tab 1",
				Command = new Command(async () => await Shell.Current.GoToAsync("//tab1"))
			};
		}
	}
}