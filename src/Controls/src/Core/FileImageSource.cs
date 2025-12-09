#nullable disable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls;

/// <summary>An <see cref="Microsoft.Maui.Controls.ImageSource"/> that reads an image from a file.</summary>
[System.ComponentModel.TypeConverter(typeof(FileImageSourceConverter))]
public sealed partial class FileImageSource : ImageSource
{
	/// <summary>Bindable property for <see cref="File"/>.</summary>
	public static readonly BindableProperty FileProperty = BindableProperty.Create(nameof(File), typeof(string), typeof(FileImageSource), default(string));

	/// <summary>Indicates whether the <see cref="Microsoft.Maui.Controls.FileImageSource.File"/> property is null or empty.</summary>
	public override bool IsEmpty => string.IsNullOrEmpty(File);

	/// <summary>Gets or sets the file from which this <see cref="Microsoft.Maui.Controls.FileImageSource"/> will load an image.</summary>
	public string File
	{
		get => (string)GetValue(FileProperty);
		set => SetValue(FileProperty, value);
	}

	/// <summary>Request a cancel of the ImageSource loading.</summary>
	/// <returns>An awaitable Task.</returns>
	/// <remarks>overridden for FileImageSource. FileImageSource are not cancellable, so this will always returns a completed Task with <see langword="false"/> as Result.</remarks>
	public override Task<bool> Cancel() => Task.FromResult(false);

	/// <summary>Returns the path to the file for the image, prefixed with the string, "File: ".</summary>
	public override string ToString() => $"File: {File}";

	public static implicit operator FileImageSource(string file) => (FileImageSource)FromFile(file);

	public static implicit operator string(FileImageSource file) => file?.File;

	protected override void OnPropertyChanged(string propertyName = null)
	{
		if (propertyName == nameof(File))
		{
			OnSourceChanged();
		}
		base.OnPropertyChanged(propertyName);
	}
}