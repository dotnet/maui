namespace MauiApp._1.Services;

/// <summary>
/// Modal Error Handler.
/// </summary>
public class ModalErrorHandler : IErrorHandler
{
	/// <summary>
	/// Handle error in UI.
	/// </summary>
	/// <param name="ex">Exception.</param>
	public void HandleError(Exception ex)
	{
		lock (this)
		{
			Shell.Current?.DisplayAlert("Error", ex.Message, "OK");
		}
	}
}