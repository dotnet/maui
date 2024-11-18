using CommunityToolkit.Mvvm.Input;
using MauiApp._1.Models;

namespace MauiApp._1.PageModels;

public interface IProjectTaskPageModel
{
	IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
	bool IsBusy { get; }
}