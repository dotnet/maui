#nullable disable
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/FileImageSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.FileImageSource']/Docs/*" />
	[System.ComponentModel.TypeConverter(typeof(FileImageSourceConverter))]
	public sealed partial class FileImageSource : ImageSource
	{
		/// <summary>Bindable property for <see cref="File"/>.</summary>
		public static readonly BindableProperty FileProperty = BindableProperty.Create("File", typeof(string), typeof(FileImageSource), default(string));

		/// <include file="../../docs/Microsoft.Maui.Controls/FileImageSource.xml" path="//Member[@MemberName='IsEmpty']/Docs/*" />
		public override bool IsEmpty => string.IsNullOrEmpty(File);

		/// <include file="../../docs/Microsoft.Maui.Controls/FileImageSource.xml" path="//Member[@MemberName='File']/Docs/*" />
		public string File
		{
			get { return (string)GetValue(FileProperty); }
			set { SetValue(FileProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/FileImageSource.xml" path="//Member[@MemberName='Cancel']/Docs/*" />
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/FileImageSource.xml" path="//Member[@MemberName='ToString']/Docs/*" />
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
			return file?.File;
		}

		protected override void OnPropertyChanged(string propertyName = null)
		{
			if (propertyName == FileProperty.PropertyName)
				OnSourceChanged();
			base.OnPropertyChanged(propertyName);
		}
	}
}