// this file tests: https://github.com/dotnet/maui/issues/14158

using System;

namespace Microsoft.Maui.Controls;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[Obsolete("This is not meant to be used by anything and is just for tests.", true)]
internal sealed class XmlnsDefinitionAttribute : Attribute
{
}
