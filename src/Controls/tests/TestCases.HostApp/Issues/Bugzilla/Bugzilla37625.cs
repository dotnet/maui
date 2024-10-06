﻿using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 37625, "App crashes when quickly adding/removing Image views (Windows UWP)")]
	public class Bugzilla37625 : TestContentPage
	{
		protected override async void Init()
		{
			int retry = 5;
			while (retry-- >= 0)
			{
				var imageUri = new Uri("https://raw.githubusercontent.com/xamarin/Xamarin.Forms/main/Microsoft.Maui.Controls.ControlGallery.Android/Assets/WebImages/XamarinLogo.png");
				Content = new Image() { Source = new UriImageSource() { Uri = imageUri }, BackgroundColor = Colors.Black, AutomationId = "success" };

				await Task.Delay(50);
			}
		}
	}
}
