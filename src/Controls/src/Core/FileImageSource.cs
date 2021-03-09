using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	[TypeConverter(typeof(FileImageSourceConverter))]
	public sealed class FileImageSource : ImageSource
	{
		public static readonly BindableProperty FileProperty = BindableProperty.Create("File", typeof(string), typeof(FileImageSource), default(string));

		public override bool IsEmpty => string.IsNullOrEmpty(File);

		public string File
		{
			get { return (string)GetValue(FileProperty); }
			set { SetValue(FileProperty, value); }
		}

		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		public override string ToString()
		{
			return $"File: {File}";
		}

		public static implicit operator FileImageSource(string file)
		{
			return (FileImageSource)FromFile(file);
		}

		public static implicit operator string(FileImageSource file)
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