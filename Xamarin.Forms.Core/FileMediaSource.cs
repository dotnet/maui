using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms
{
	[TypeConverter(typeof(FileMediaSourceConverter))]
	public sealed class FileMediaSource : MediaSource
	{
		public static readonly BindableProperty FileProperty = BindableProperty.Create(nameof(File), typeof(string), typeof(FileMediaSource), default(string));

		public string File
		{
			get { return (string)GetValue(FileProperty); }
			set { SetValue(FileProperty, value); }
		}

		public override string ToString()
		{
			return $"File: {File}";
		}

		public static implicit operator FileMediaSource(string file)
		{
			return (FileMediaSource)FromFile(file);
		}

		public static implicit operator string(FileMediaSource file)
		{
			return file != null ? file.File : null;
		}

		protected override void OnPropertyChanged(string propertyName = null)
		{
			if (propertyName == FileProperty.PropertyName)
				OnSourceChanged();
			base.OnPropertyChanged(propertyName);
		}
	}
}
