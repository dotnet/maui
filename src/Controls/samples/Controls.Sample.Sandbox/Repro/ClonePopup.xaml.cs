using CommunityToolkit.Maui.Views;

namespace LabelTextWrapTest;
using System;


public partial class ClonePopup : Popup
{
    
    public ClonePopup() =>
        InitializeComponent();
    
    private void CancelButton_Clicked(object sender, EventArgs e)
    {
        Close();
    }


}