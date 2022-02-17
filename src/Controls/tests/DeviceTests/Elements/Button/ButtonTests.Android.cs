#nullable enable
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Google.Android.Material.Button;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = global::Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
		AppCompatButton GetNativeButton(ButtonHandler buttonHandler) =>
			(AppCompatButton)buttonHandler.PlatformView;

		Task<string?> GetNativeText(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() => GetNativeButton(buttonHandler).Text);
		}
	}
}
