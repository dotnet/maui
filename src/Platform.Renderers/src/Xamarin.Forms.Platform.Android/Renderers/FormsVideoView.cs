using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Media;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal class FormsVideoView : VideoView
	{
		public FormsVideoView(Context context) : base(context)
		{
		}

		public event EventHandler MetadataRetrieved;

		public override void SetVideoPath(string path)
		{
			base.SetVideoPath(path);

			if (System.IO.File.Exists(path))
			{
				var retriever = new MediaMetadataRetriever();

				Task.Run(() =>
				{
					retriever.SetDataSource(path);
					ExtractMetadata(retriever);
					MetadataRetrieved?.Invoke(this, EventArgs.Empty);
				});
			}
		}

		void ExtractMetadata(MediaMetadataRetriever retriever)
		{
			int videoWidth = 0;
			if (int.TryParse(retriever.ExtractMetadata(MetadataKey.VideoWidth), out videoWidth))
			{
				VideoWidth = videoWidth;
			}

			int videoHeight = 0;
			if (int.TryParse(retriever.ExtractMetadata(MetadataKey.VideoHeight), out videoHeight))
			{
				VideoHeight = videoHeight;
			}

			long durationMS;
			string durationString = retriever.ExtractMetadata(MetadataKey.Duration);

			if (!string.IsNullOrEmpty(durationString) && long.TryParse(durationString, out durationMS))
			{
				DurationTimeSpan = TimeSpan.FromMilliseconds(durationMS);
			}
			else
			{
				DurationTimeSpan = null;
			}
		}

		public override void SetVideoURI(global::Android.Net.Uri uri, IDictionary<string, string> headers)
		{
			GetMetaData(uri, headers);
			base.SetVideoURI(uri, headers);
		}

		public override void SetVideoURI(global::Android.Net.Uri uri)
		{
			GetMetaData(uri, new Dictionary<string, string>());
			base.SetVideoURI(uri);
		}

		void GetMetaData(global::Android.Net.Uri uri, IDictionary<string, string> headers)
		{
			Task.Run(() =>
			{
				var retriever = new MediaMetadataRetriever();

				if (uri.Scheme != null && uri.Scheme.StartsWith("http") && headers != null)
				{
					retriever.SetDataSource(uri.ToString(), headers);
				}
				else
				{
					retriever.SetDataSource(Context, uri);
				}

				ExtractMetadata(retriever);

				MetadataRetrieved?.Invoke(this, EventArgs.Empty);
			});
		}

		public int VideoHeight { get; private set; }

		public int VideoWidth { get; private set; }

		public TimeSpan? DurationTimeSpan { get; private set; }

		public TimeSpan Position
		{
			get
			{
				return TimeSpan.FromMilliseconds(CurrentPosition);
			}
		}
	}
}