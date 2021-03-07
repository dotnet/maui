namespace Microsoft.Maui
{
	public interface IEntry : IView, IText
	{
		bool IsPassword { get; }
		bool IsTextPredictionEnabled { get; }
		bool IsReadOnly { get; }
	}
}