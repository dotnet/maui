using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	public partial class CheckBoxTests
	{
		MauiCheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			checkBoxHandler.PlatformView;

		Task<float> GetPlatformOpacity(CheckBoxHandler checkBoxHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeCheckBox(checkBoxHandler);
				return (float)nativeView.Alpha;
			});
		}
	}
}