using System.Collections.ObjectModel;
using System.Diagnostics;
using AllTheLists.Models;
using AllTheLists.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Adapters;

namespace AllTheLists.ViewModels;

public partial class CheckInsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CheckIn> _checkIns;
    public ObservableCollectionAdapter<CheckIn> CheckInsAdapter { get; private set; }

    public CheckInsViewModel()
    {
        CheckIns = new ObservableCollection<CheckIn>(MockDataService.GenerateCheckIns());
        CheckInsAdapter = new ObservableCollectionAdapter<CheckIn>(CheckIns);
    }
}
