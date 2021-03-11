using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Graphics
{
    public abstract class AbstractFontService : IFontService
    {
        private readonly string _defaultFontStyleName;
        private readonly string _secondaryFontStyleName;
        private readonly string _tertiaryFontStyleName;

        private readonly Dictionary<string, IFontStyle> _fontCache = new Dictionary<string, IFontStyle>();
        private IFontStyle _defaultFontStyle;
        private bool _buildingDefaultStyle;

        public abstract IFontFamily[] GetFontFamilies();

        protected AbstractFontService(
            string defaultFontStyleName = "Arial",
            string secondaryFontStyleName = "Helvetica",
            string tertiaryFontStyleName = null)
        {
            _defaultFontStyleName = defaultFontStyleName;
            _secondaryFontStyleName = secondaryFontStyleName;
            _tertiaryFontStyleName = tertiaryFontStyleName;
        }

        public virtual IFontStyle GetDefaultFontStyle()
        {
            if (_defaultFontStyle == null)
            {
                if (_buildingDefaultStyle)
                    return null;

                _buildingDefaultStyle = true;
                _defaultFontStyle = GetFontStyleById(_defaultFontStyleName);

                if (_defaultFontStyle == null && !string.IsNullOrEmpty(_secondaryFontStyleName))
                {
                    _defaultFontStyle = GetFontStyleById(_secondaryFontStyleName);
                }

                if (_defaultFontStyle == null && !string.IsNullOrEmpty(_tertiaryFontStyleName))
                {
                    _defaultFontStyle = GetFontStyleById(_tertiaryFontStyleName);
                }

                if (_defaultFontStyle == null)
                {
                    var families = GetFontFamilies();
                    if (families != null && families.Length > 0)
                    {
                        _defaultFontStyle = families[0].GetFontStyles()[0];
                    }
                }

                _buildingDefaultStyle = false;
            }

            return _defaultFontStyle;
        }

        public IFontStyle GetFontStyleById(string id)
        {
            if (id == null)
                return null;

            if (!_fontCache.TryGetValue(id, out var fontStyle))
            {
                var families = GetFontFamilies();
                for (int f = 0; f < families.Length && fontStyle == null; f++)
                {
                    var family = families[f];

                    if (id.Equals(family.Name))
                    {
                        fontStyle = family.GetFontStyles()[0];
                    }
                    else
                    {
                        var styles = family.GetFontStyles();
                        if (styles != null)
                        {
                            for (int s = 0; s < styles.Length && fontStyle == null; s++)
                            {
                                var style = styles[s];
                                if (id.Equals(style.Id, StringComparison.OrdinalIgnoreCase))
                                    fontStyle = style;
                            }
                        }
                    }
                }

                if (fontStyle != null)
                    _fontCache[id] = fontStyle;
                else
                    _fontCache[id] = GetDefaultFontStyle();
            }

            return fontStyle;
        }
    }
}