using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui;
using System.Maui.Xaml;

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