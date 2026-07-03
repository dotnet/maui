namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "SafeAreaEdges independent handling for parent and child controls", PlatformAffected.iOS, issueTestNumber: 9)]
public partial class Issue28986_ParentChildTest : ContentPage
{
    bool _parentTopEnabled = true;
    bool _parentBottomEnabled = false;
    bool _childBottomEnabled = true;

    public Issue28986_ParentChildTest()
    {
        InitializeComponent();
        UpdateParentGridSafeAreaEdges();
        UpdateStatusLabel();
    }

    void OnToggleParentTop(object sender, EventArgs e)
    {
        _parentTopEnabled = !_parentTopEnabled;
        UpdateParentGridSafeAreaEdges();
        UpdateStatusLabel();
    }

    void OnToggleParentBottom(object sender, EventArgs e)
    {
        _parentBottomEnabled = !_parentBottomEnabled;
        UpdateParentGridSafeAreaEdges();
        UpdateStatusLabel();
    }

    void OnToggleChildBottom(object sender, EventArgs e)
    {
        _childBottomEnabled = !_childBottomEnabled;

        // Toggle between Bottom=Container and Bottom=None
        ChildGrid.SafeAreaEdges = _childBottomEnabled
            ? new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.Container)
            : new SafeAreaEdges(SafeAreaRegions.None);

        UpdateStatusLabel();
    }

    void UpdateParentGridSafeAreaEdges()
    {
        // Build parent grid SafeAreaEdges based on top and bottom flags
        SafeAreaRegions top = _parentTopEnabled ? SafeAreaRegions.Container : SafeAreaRegions.None;
        SafeAreaRegions bottom = _parentBottomEnabled ? SafeAreaRegions.Container : SafeAreaRegions.None;

        ParentGrid.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, top, SafeAreaRegions.None, bottom);
    }

    void UpdateStatusLabel()
    {
        var parentTop = _parentTopEnabled ? "Container" : "None";
        var parentBottom = _parentBottomEnabled ? "Container" : "None";
        var childBottom = _childBottomEnabled ? "Container" : "None";
        StatusLabel.Text = $"Parent: Top={parentTop}, Bottom={parentBottom} | Child: Bottom={childBottom}";
    }
}
