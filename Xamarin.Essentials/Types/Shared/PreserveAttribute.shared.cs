using System;

namespace Xamarin.Essentials
{
    internal sealed class PreserveAttribute : Attribute
    {
        public bool AllMembers { get; set; }

        public bool Conditional { get; set; }
    }
}
