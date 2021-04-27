#nullable enable
namespace Microsoft.Maui
{
	public interface IFileImageSource : IImageSource
	{
		string File { get; }
	}
}