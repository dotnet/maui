using System.Reflection;
using System.Runtime.CompilerServices;

#if DEBUG && !STRONG_NAME
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.Xaml.UnitTests")]
#endif