namespace MauiApp._1.Services;

/// <summary>
/// Error Handler Service.
/// </summary>
public interface IErrorHandler
{
	/// <summary>
	/// Handle error in UI.
	/// </summary>
	/// <param name="ex">Exception being thrown.</param>
	void HandleError(Exception ex);
}