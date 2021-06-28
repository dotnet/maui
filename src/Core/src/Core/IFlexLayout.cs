using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Flex = Microsoft.Maui.Layouts.Flex;

namespace Microsoft.Maui
{
	public interface IFlexLayout : ILayout
	{
		FlexDirection Direction { get; }
		FlexJustify JustifyContent { get; }
		FlexAlignContent AlignContent { get; }
		FlexAlignItems AlignItems { get; }
		FlexPosition Position { get; }
		FlexWrap Wrap { get; }

		int GetOrder(IView view);
		float GetGrow(IView view);
		float GetShrink(IView view);
		FlexAlignSelf GetAlignSelf(IView view);
		FlexBasis GetBasis(IView view);

		Rectangle GetFlexFrame(IView view);

		void Layout(double width, double height);
	}
}