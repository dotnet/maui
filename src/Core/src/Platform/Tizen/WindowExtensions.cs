using Microsoft.Extensions.DependencyInjection;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;

namespace Microsoft.Maui
{
	public static class WindowExtensions
	{
		public static void SetContent(this Window nativeWindow, View content)
		{
			content.HeightSpecification = LayoutParamPolicies.MatchParent;
			content.WidthSpecification = LayoutParamPolicies.MatchParent;
			content.HeightResizePolicy = ResizePolicyType.FillToParent;
			content.WidthResizePolicy = ResizePolicyType.FillToParent;

			MauiApplication.Current.ModalStack.Clear();
			MauiApplication.Current.ModalStack.Push(content, true);
		}
	}
}
