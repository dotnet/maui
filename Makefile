UNAME_S:=$(shell sh -c 'uname -s 2>/dev/null || echo not')
ifeq ($(UNAME_S),Darwin)
	MONOHOME=/Library/Frameworks/Mono.framework/Libraries/mono
endif
ifeq ($(UNAME_S),Linux)
	MONOHOME=/usr/lib/mono
endif

DOTNETPCL=$(MONOHOME)/xbuild-frameworks/.NETPortable/v4.5/Profile/Profile259/
CORE=Xamarin.Forms.Core
COREASSEMBLY=$(CORE)/bin/Release/$(CORE).dll
XAML=Xamarin.Forms.Xaml
XAMLASSEMBLY=$(XAML)/bin/Release/$(XAML).dll
MAPS=Xamarin.Forms.Maps
MAPSASSEMBLY=$(MAPS)/bin/Release/$(MAPS).dll
MDOC=mono tools/mdoc/mdoc.exe

docs: $(CORE).docs $(MAPS).docs $(XAML).docs

$(CORE).docs: $(COREASSEMBLY)
	$(MDOC) update --delete -o docs/$(CORE) $(COREASSEMBLY) -L $(DOTNETPCL)

$(XAML).docs: $(XAMLASSEMBLY)
	$(MDOC) update --delete -o docs/$(XAML) $(XAMLASSEMBLY) -L $(DOTNETPCL)

$(MAPS).docs: $(MAPSASSEMBLY)
	$(MDOC) update --delete -o docs/$(MAPS) $(MAPSASSEMBLY) -L $(DOTNETPCL)

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
