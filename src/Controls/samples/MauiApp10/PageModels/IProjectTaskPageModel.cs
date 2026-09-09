using CommunityToolkit.Mvvm.Input;
using MauiApp10.Models;

namespace MauiApp10.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}