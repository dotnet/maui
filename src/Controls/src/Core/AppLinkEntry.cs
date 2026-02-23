#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <summary>A deep application link in an app link search index.</summary>
	public class AppLinkEntry : Element, IAppLinkEntry
	{
		readonly Dictionary<string, string> keyValues;

		/// <summary>Creates a new <see cref="Microsoft.Maui.Controls.AppLinkEntry"/> with default values.</summary>
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

		/// <summary>Gets or sets an application-specific URI that uniquely describes content within an app. This is a bindable property.</summary>
		public Uri AppLinkUri
		{
			get { return (Uri)GetValue(AppLinkUriProperty); }
			set { SetValue(AppLinkUriProperty, value); }
		}

		/// <summary>Gets or sets a description that appears with the item in search results. This is a bindable property.</summary>
		public string Description
		{
			get { return (string)GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}

		/// <summary>Gets or sets a value that tells whether the item that is identified by the link entry is currently open.</summary>
		/// <remarks>Application developers can set this value in <see cref="Microsoft.Maui.Controls.Application.PageAppearing"/> and <see cref="Microsoft.Maui.Controls.Application.PageDisappearing"/> methods to control whether the app link is shown for indexing or Handoff.</remarks>
		public bool IsLinkActive
		{
			get { return (bool)GetValue(IsLinkActiveProperty); }
			set { SetValue(IsLinkActiveProperty, value); }
		}

		/// <summary>Gets a dictionary of application-specific key-value pairs.</summary>
		/// <remarks>The standard keys are <c>contentType</c>, <c>associatedWebPage</c>, and <c>shouldAddToPublicIndex</c>.</remarks>
		public IDictionary<string, string> KeyValues
		{
			get { return keyValues; }
		}

		/// <summary>Gets or sets a small image that appears with the item in search results. This is a bindable property.</summary>
		public ImageSource Thumbnail
		{
			get { return (ImageSource)GetValue(ThumbnailProperty); }
			set { SetValue(ThumbnailProperty, value); }
		}

		/// <summary>Gets or sets the title of the item. This is a bindable property.</summary>
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <summary>Creates and returns a new <see cref="Microsoft.Maui.Controls.AppLinkEntry"/> for the specified <paramref name="uri"/>.</summary>
		/// <param name="uri">A URI that can be parsed by the target appliction to recreate a specific state.</param>
		public static AppLinkEntry FromUri(Uri uri)
		{
			var appEntry = new AppLinkEntry();
			appEntry.AppLinkUri = uri;
			return appEntry;
		}

		/// <summary>Returns a string representation of this <see cref="Microsoft.Maui.Controls.AppLinkEntry"/>.</summary>
		/// <returns>A  string representation of this <see cref="Microsoft.Maui.Controls.AppLinkEntry"/>.</returns>
		public override string ToString()
		{
			return AppLinkUri.ToString();
		}
	}
}