using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms
{
	[TypeConverter(typeof(MediaSourceConverter))]
	public abstract class MediaSource : Element
	{
		readonly WeakEventManager _weakEventManager = new WeakEventManager();

		public static MediaSource FromFile(string file)
		{
			return new FileMediaSource { File = file };
		}

		public static MediaSource FromUri(Uri uri)
		{
			if (!uri.IsAbsoluteUri)
				throw new ArgumentException("uri is relative");
			return new UriMediaSource { Uri = uri };
		}

		public static implicit operator MediaSource(string source)
		{
			Uri uri;
			return Uri.TryCreate(source, UriKind.Absolute, out uri) && uri.Scheme != "file" ? FromUri(uri) : FromFile(source);
		}

		public static implicit operator MediaSource(Uri uri)
		{
			if (uri is null)
				return null;

			if (!uri.IsAbsoluteUri)
				throw new ArgumentException("uri is relative");
			return FromUri(uri);
		}

		protected void OnSourceChanged()
		{
			_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(SourceChanged));
		}

		internal event EventHandler SourceChanged
		{
			add { _weakEventManager.AddEventHandler(value); }
			remove { _weakEventManager.RemoveEventHandler(value); }
		}
	}
}
