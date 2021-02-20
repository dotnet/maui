using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

namespace Xamarin.Platform.Handlers.DeviceTests
{
	public partial class LabelHandlerTests
	{
		UILabel GetNativeLabel(LabelHandler labelHandler) =>
			(UILabel)labelHandler.View;

		string GetNativeText(LabelHandler labelHandler) =>
			 GetNativeLabel(labelHandler).Text;

		Color GetNativeTextColor(LabelHandler labelHandler) =>
			 GetNativeLabel(labelHandler).TextColor.ToColor();

		Task ValidateNativeBackgroundColor(ILabel label, Color color)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeLabel(CreateHandler(label)).AssertContainsColor(color);
			});
		}
	}
}