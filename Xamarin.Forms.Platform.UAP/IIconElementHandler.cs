using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	public interface IIconElementHandler : IRegisterable
	{
		Task<IconElement> LoadIconElementAsync(ImageSource imagesource, CancellationToken cancellationToken = default(CancellationToken));
	}
}
