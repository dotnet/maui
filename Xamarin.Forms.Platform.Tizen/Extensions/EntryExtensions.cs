using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	internal static class EntryExtensions
	{
		internal static InputPanelReturnKeyType ToInputPanelReturnKeyType(this ReturnType returnType)
		{
			switch (returnType)
			{
				case ReturnType.Go:
					return InputPanelReturnKeyType.Go;
				case ReturnType.Next:
					return InputPanelReturnKeyType.Next;
				case ReturnType.Send:
					return InputPanelReturnKeyType.Send;
				case ReturnType.Search:
					return InputPanelReturnKeyType.Search;
				case ReturnType.Done:
					return InputPanelReturnKeyType.Done;
				case ReturnType.Default:
					return InputPanelReturnKeyType.Default;
				default:
					throw new System.NotImplementedException($"ReturnType {returnType} not supported");
			}
		}

	}
}
