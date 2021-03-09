using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace DualScreen
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Master : CollectionView
    {
        public Master()
        {
            InitializeComponent();
            ItemsSource = Enumerable.Range(1, 100)
                .Select(x => new MasterDetailsItem(x))
                .ToList();
        }
    }
}