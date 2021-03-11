using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Graphics.Text.Renderer
{
    public static class SimpleCssParser
    {
        public static Dictionary<string, string> Parse(string css)
        {
            if (string.IsNullOrEmpty(css))
                return null;

            var values = new Dictionary<string, string>();

            var entries = css.Split(';');
            foreach (var entry in entries)
            {
                try
                {
                    var index = entry.IndexOf(':');
                    if (index > 0)
                    {
                        var key = entry.Substring(0, index).Trim();
                        var value = entry.Substring(index + 1).Trim();
                        values[key] = value;
                    }
                }
                catch (Exception exc)
                {
                    // todo: this should be replaced with a logger.
                    System.Diagnostics.Debug.WriteLine(exc.Message);
                }
            }

            return values;
        }
    }
}