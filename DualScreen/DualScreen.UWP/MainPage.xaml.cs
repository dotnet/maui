using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using global::Windows.Foundation;
using global::Windows.Foundation.Collections;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls;
using global::Windows.UI.Xaml.Controls.Primitives;
using global::Windows.UI.Xaml.Data;
using global::Windows.UI.Xaml.Input;
using global::Windows.UI.Xaml.Media;
using global::Windows.UI.Xaml.Navigation;

namespace DualScreen.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new DualScreen.App());
        }
    }
}
