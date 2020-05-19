using System;
using Xamarin.UITest;

namespace System.Maui.Core.UITests
{
	internal sealed class ViewContainerRemote : BaseViewContainerRemote
	{
		public ViewContainerRemote (IApp app, Enum formsType, string platformViewType)
			: base (app, formsType, platformViewType) { }
	}
}