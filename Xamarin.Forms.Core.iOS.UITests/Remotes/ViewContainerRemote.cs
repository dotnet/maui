using System;

using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal sealed class ViewContainerRemote : BaseViewContainerRemote
	{
		public ViewContainerRemote (IApp app, Enum formsType, string platformViewType)
			: base (app, formsType, platformViewType) { }
	}
}