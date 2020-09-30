using System;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	internal sealed class ViewContainerRemote : BaseViewContainerRemote
	{
		public ViewContainerRemote(IApp app, Enum formsType, string platformViewType)
			: base(app, formsType, platformViewType) { }
	}
}