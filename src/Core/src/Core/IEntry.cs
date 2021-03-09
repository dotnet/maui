namespace Microsoft.Maui
{
	public interface IEntry : IView, IText, ITextInput
	{
		bool IsPassword { get; }
		bool IsTextPredictionEnabled { get; }
	}
}