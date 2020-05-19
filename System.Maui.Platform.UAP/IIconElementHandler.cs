using System.Threading;
using System.Threading.Tasks;
using global::Windows.UI.Xaml.Controls;

namespace System.Maui.Platform.UWP
{
	public interface IIconElementHandler : IRegisterable
	{
		Task<Microsoft.UI.Xaml.Controls.IconSource> LoadIconSourceAsync(ImageSource imagesource, CancellationToken cancellationToken = default(CancellationToken));
		Task<IconElement> LoadIconElementAsync(ImageSource imagesource, CancellationToken cancellationToken = default(CancellationToken));
	}
}