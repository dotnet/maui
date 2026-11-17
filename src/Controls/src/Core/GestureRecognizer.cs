using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <summary>The base class for all gesture recognizers.</summary>
	public class GestureRecognizer : Element, IGestureRecognizer
	{
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(DragGestureRecognizer))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(DropGestureRecognizer))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(TapGestureRecognizer))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(PanGestureRecognizer))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(PinchGestureRecognizer))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(SwipeGestureRecognizer))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(PointerGestureRecognizer))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(LongPressGestureRecognizer))]
		public GestureRecognizer()
		{
		}
	}
}
