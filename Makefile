CORE=Xamarin.Forms.Core
COREASSEMBLY=$(CORE)/bin/Release/$(CORE).dll
XAML=Xamarin.Forms.Xaml
XAMLASSEMBLY=$(XAML)/bin/Release/$(XAML).dll
MAPS=Xamarin.Forms.Maps
MAPSASSEMBLY=$(MAPS)/bin/Release/$(MAPS).dll
# MDOC=/Library/Frameworks/Mono.framework/Versions/2.10.12/bin/mdoc
MDOC=mdoc

docs: $(CORE).docs $(MAPS).docs $(XAML).docs

$(CORE).docs: $(COREASSEMBLY)
	$(MDOC) update --delete -o docs/$(CORE) $(COREASSEMBLY) -L /Library/Frameworks/Mono.framework/Libraries/mono/xbuild-frameworks/.NETPortable/v4.5/Profile/Profile259/

$(XAML).docs: $(XAMLASSEMBLY)
	$(MDOC) update --delete -o docs/$(XAML) $(XAMLASSEMBLY) -L /Library/Frameworks/Mono.framework/Libraries/mono/xbuild-frameworks/.NETPortable/v4.5/Profile/Profile259/

$(MAPS).docs: $(MAPSASSEMBLY)
	$(MDOC) update --delete -o docs/$(MAPS) $(MAPSASSEMBLY) -L /Library/Frameworks/Mono.framework/Libraries/mono/xbuild-frameworks/.NETPortable/v4.5/Profile/Profile259/

$(COREASSEMBLY): .FORCE
	xbuild /property:Configuration=Release Xamarin.Forms.Core/Xamarin.Forms.Core.csproj

$(XAMLASSEMBLY): .FORCE
	xbuild /property:Configuration=Release Xamarin.Forms.Xaml/Xamarin.Forms.Xaml.csproj

$(MAPSASSEMBLY): .FORCE
	xbuild /property:Configuration=Release Xamarin.Forms.Maps/Xamarin.Forms.Maps.csproj

htmldocs: docs
	$(MDOC) export-html -o htmldocs docs/*


xmldocs: docs 
	$(MDOC) export-msxdoc -o docs/$(CORE).xml docs/$(CORE)
	$(MDOC) export-msxdoc -o docs/$(XAML).xml docs/$(XAML)
	$(MDOC) export-msxdoc -o docs/$(MAPS).xml docs/$(MAPS)
.FORCE:

.PHONY: .FORCE $(CORE).docs $(MAPS).docs $(XAML).docs htmldocs xmldocs
