using System;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	public class AppLinkEntry : Element, IAppLinkEntry
	{
		readonly Dictionary<string, string> keyValues;

		public AppLinkEntry()
		{
			keyValues = new Dictionary<string, string>();
		}

		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(AppLinkEntry), default(string));

		public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(nameof(Description), typeof(string), typeof(AppLinkEntry), default(string));

		public static readonly BindableProperty ThumbnailProperty = BindableProperty.Create(nameof(Thumbnail), typeof(ImageSource), typeof(AppLinkEntry), default(ImageSource));

		public static readonly BindableProperty AppLinkUriProperty = BindableProperty.Create(nameof(AppLinkUri), typeof(Uri), typeof(AppLinkEntry), null);

		public static readonly BindableProperty IsLinkActiveProperty = BindableProperty.Create(nameof(IsLinkActive), typeof(bool), typeof(AppLinkEntry), false);

		public Uri AppLinkUri
		{
			get { return (Uri)GetValue(AppLinkUriProperty); }
			set { SetValue(AppLinkUriProperty, value); }
		}

		public string Description
		{
			get { return (string)GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}

		public bool IsLinkActive
		{
			get { return (bool)GetValue(IsLinkActiveProperty); }
			set { SetValue(IsLinkActiveProperty, value); }
		}

		public IDictionary<string, string> KeyValues
		{
			get { return keyValues; }
		}

		public ImageSource Thumbnail
		{
			get { return (ImageSource)GetValue(ThumbnailProperty); }
			set { SetValue(ThumbnailProperty, value); }
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static AppLinkEntry FromUri(Uri uri)
		{
			var appEntry = new AppLinkEntry();
			appEntry.AppLinkUri = uri;
			return appEntry;
		}

		public override string ToString()
		{
			return AppLinkUri.ToString();
		}
	}
}