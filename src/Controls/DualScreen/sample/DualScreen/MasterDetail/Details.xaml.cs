using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DualScreen
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Details : Grid
    {
        public Details()
        {
            InitializeComponent();
        }
    }
}