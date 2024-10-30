using CommunityToolkit.Mvvm.ComponentModel;

namespace AllTheLists.Models.Learning;

public partial class Unit : ObservableObject
{
    [ObservableProperty]
    private int _unitNumber;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _subTitle;

    [ObservableProperty]
    private string _icon;

    [ObservableProperty]
    private List<Chapter> _chapters;

    [ObservableProperty]
    private bool _isShowingLessons;

}
