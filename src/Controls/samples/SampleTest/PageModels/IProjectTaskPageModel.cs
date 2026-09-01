using CommunityToolkit.Mvvm.Input;
using SampleTest.Models;

namespace SampleTest.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}