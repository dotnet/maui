UNAME_S:=$(shell sh -c 'uname -s 2>/dev/null || echo not')
ifeq ($(UNAME_S),Darwin)
	MONOHOME=/Library/Frameworks/Mono.framework/Libraries/mono
endif
ifeq ($(UNAME_S),Linux)
	MONOHOME=/usr/lib/mono
endif

DOTNETPCL=$(MONOHOME)/xbuild-frameworks/.NETPortable/v4.5/Profile/Profile259/
CORE=Xamarin.Forms.Core
COREASSEMBLY=$(CORE)/bin/Release/netstandard2.0/$(CORE).dll
XAML=Xamarin.Forms.Xaml
XAMLASSEMBLY=$(XAML)/bin/Release/netstandard2.0/$(XAML).dll
MAPS=Xamarin.Forms.Maps
MAPSASSEMBLY=$(MAPS)/bin/Release/netstandard2.0/$(MAPS).dll
PAGES=Xamarin.Forms.Pages
PAGESASSEMBLY=$(PAGES)/bin/Release/netstandard2.0/$(PAGES).dll
MDOC=mdoc

docs: $(CORE).docs $(MAPS).docs $(XAML).docs $(PAGES).docs

$(CORE).docs: $(COREASSEMBLY)
	$(MDOC) update --delete -o docs/$(CORE) $(COREASSEMBLY) -L $(DOTNETPCL)

$(XAML).docs: $(XAMLASSEMBLY)
	$(MDOC) update --delete -o docs/$(XAML) $(XAMLASSEMBLY) -L $(DOTNETPCL)

$(MAPS).docs: $(MAPSASSEMBLY)
	$(MDOC) update --delete -o docs/$(MAPS) $(MAPSASSEMBLY) -L $(DOTNETPCL)

$(PAGES).docs: $(PAGESASSEMBLY)
	$(MDOC) update --delete -o docs/$(PAGES) $(PAGESASSEMBLY) -L $(DOTNETPCL)

$(COREASSEMBLY): .FORCE
	msbuild /property:Configuration=Release Xamarin.Forms.Core/Xamarin.Forms.Core.csproj

$(XAMLASSEMBLY): .FORCE
	msbuild /property:Configuration=Release Xamarin.Forms.Xaml/Xamarin.Forms.Xaml.csproj

$(MAPSASSEMBLY): .FORCE
	msbuild /property:Configuration=Release Xamarin.Forms.Maps/Xamarin.Forms.Maps.csproj

$(PAGESASSEMBLY): .FORCE
	msbuild /property:Configuration=Release Xamarin.Forms.Pages/Xamarin.Forms.Pages.csproj

htmldocs: docs
	$(MDOC) export-html -o htmldocs docs/*


xmldocs: docs 
	$(MDOC) export-msxdoc -o docs/$(CORE).xml docs/$(CORE)
	$(MDOC) export-msxdoc -o docs/$(XAML).xml docs/$(XAML)
	$(MDOC) export-msxdoc -o docs/$(MAPS).xml docs/$(MAPS)
	$(MDOC) export-msxdoc -o docs/$(PAGES).xml docs/$(PAGES)
.FORCE:

.PHONY: .FORCE $(CORE).docs $(MAPS).docs $(XAML).docs $(PAGES).docs htmldocs xmldocs
