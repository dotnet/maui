using System;
using Xamarin.UITest;

namespace Microsoft.Maui.Controls.Compatibility.UITests
{
	using IApp = Xamarin.UITest.IApp;
	internal sealed class ViewContainerRemote : BaseViewContainerRemote
	{
		public ViewContainerRemote(Xamarin.UITest.IApp app, Enum formsType, string platformViewType)
			: base(app, formsType, platformViewType) { }
	}
}