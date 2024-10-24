namespace MauiApp._1.Services;

/// <summary>
/// Modal Error Handler.
/// </summary>
public class ModalErrorHandler : IErrorHandler
{
	Task? _currentTask;
	/// <summary>
	/// Handle error in UI.
	/// </summary>
	/// <param name="ex">Exception.</param>
	public void HandleError(Exception ex)
	{
		DisplayAlert(ex).FireAndForgetSafeAsync();
	}

	async Task DisplayAlert(Exception ex)
	{
		// Only display one error at a time
		if (_currentTask is null)
			return;

		if (Shell.Current is Shell shell)
		{
			_currentTask = shell.DisplayAlert("Error", ex.Message, "OK");
			await (_currentTask ?? Task.CompletedTask);
			_currentTask = null;
		}
	}
}