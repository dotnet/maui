#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/AppLinkEntry.xml" path="Type[@FullName='Microsoft.Maui.Controls.AppLinkEntry']/Docs/*" />
	public class AppLinkEntry : Element, IAppLinkEntry
	{
		readonly Dictionary<string, string> keyValues;

		/// <include file="../../docs/Microsoft.Maui.Controls/AppLinkEntry.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public AppLinkEntry()
		{
			keyValues = new(StringComparer.Ordinal);
		}

		/// <summary>Bindable property for <see cref="Title"/>.</summary>
		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(AppLinkEntry), default(string));

		/// <summary>Bindable property for <see cref="Description"/>.</summary>
		public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(nameof(Description), typeof(string), typeof(AppLinkEntry), default(string));

		/// <summary>Bindable property for <see cref="Thumbnail"/>.</summary>
		public static readonly BindableProperty ThumbnailProperty = BindableProperty.Create(nameof(Thumbnail), typeof(ImageSource), typeof(AppLinkEntry), default(ImageSource));

		/// <summary>Bindable property for <see cref="AppLinkUri"/>.</summary>
		public static readonly BindableProperty AppLinkUriProperty = BindableProperty.Create(nameof(AppLinkUri), typeof(Uri), typeof(AppLinkEntry), null);

		/// <summary>Bindable property for <see cref="IsLinkActive"/>.</summary>
		public static readonly BindableProperty IsLinkActiveProperty = BindableProperty.Create(nameof(IsLinkActive), typeof(bool), typeof(AppLinkEntry), false);

		/// <include file="../../docs/Microsoft.Maui.Controls/AppLinkEntry.xml" path="//Member[@MemberName='AppLinkUri']/Docs/*" />
		public Uri AppLinkUri
		{
			get { return (Uri)GetValue(AppLinkUriProperty); }
			set { SetValue(AppLinkUriProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AppLinkEntry.xml" path="//Member[@MemberName='Description']/Docs/*" />
		public string Description
		{
			get { return (string)GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AppLinkEntry.xml" path="//Member[@MemberName='IsLinkActive']/Docs/*" />
		public bool IsLinkActive
		{
			get { return (bool)GetValue(IsLinkActiveProperty); }
			set { SetValue(IsLinkActiveProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AppLinkEntry.xml" path="//Member[@MemberName='KeyValues']/Docs/*" />
		public IDictionary<string, string> KeyValues
		{
			get { return keyValues; }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AppLinkEntry.xml" path="//Member[@MemberName='Thumbnail']/Docs/*" />
		public ImageSource Thumbnail
		{
			get { return (ImageSource)GetValue(ThumbnailProperty); }
			set { SetValue(ThumbnailProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AppLinkEntry.xml" path="//Member[@MemberName='Title']/Docs/*" />
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AppLinkEntry.xml" path="//Member[@MemberName='FromUri']/Docs/*" />
		public static AppLinkEntry FromUri(Uri uri)
		{
			var appEntry = new AppLinkEntry();
			appEntry.AppLinkUri = uri;
			return appEntry;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AppLinkEntry.xml" path="//Member[@MemberName='ToString']/Docs/*" />
		public override string ToString()
		{
			return AppLinkUri.ToString();
		}
	}
}