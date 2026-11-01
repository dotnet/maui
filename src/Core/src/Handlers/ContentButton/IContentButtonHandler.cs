#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIButton;
#elif MONOANDROID
using PlatformView = Google.Android.Material.Card.MaterialCardView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.Button;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#else
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	/// <summary>Provides access to a content button and its native view.</summary>
	public interface IContentButtonHandler : IViewHandler
	{
		/// <inheritdoc/>
		new IContentButton VirtualView { get; }

		/// <inheritdoc/>
		new PlatformView PlatformView { get; }
	}
}
