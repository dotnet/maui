#Requires -Modules Pester

Describe 'Windows XAML trimming metadata' {
  BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path

    function Get-XamlDocument([string] $relativePath) {
      [xml](Get-Content -LiteralPath (Join-Path $repoRoot $relativePath) -Raw)
    }

    function Get-XamlAttributeValue($node, [string] $localName) {
      $attribute = @($node.Attributes | Where-Object { $_.LocalName -eq $localName }) | Select-Object -First 1
      if ($null -eq $attribute) {
        return $null
      }

      return $attribute.Value
    }

    function Get-XamlElements($document, [string] $localName) {
      @($document.SelectNodes(".//*[local-name()='$localName']"))
    }

    function Get-XamlDataTemplate($document, [string] $key) {
      Get-XamlElements $document 'DataTemplate' |
        Where-Object { (Get-XamlAttributeValue $_ 'Key') -eq $key } |
        Select-Object -First 1
    }
  }

  It 'declares exact data source types for keyed DataTemplates' {
    $expectations = @(
      @{
        Path = 'src/Core/src/Platform/Windows/Styles/MauiComboBoxStyle.xaml'
        Templates = @{ ComboBoxHeader = 'maui:IPicker' }
      },
      @{
        Path = 'src/Core/src/Platform/Windows/Styles/WindowRootViewStyle.xaml'
        Templates = @{
          MauiAppTitleBarTemplateDefault = 'maui:WindowRootView'
          MauiAppTitleBarContainerTemplateDefault = 'maui:WindowRootView'
        }
      },
      @{
        Path = 'src/Controls/src/Core/Platform/Windows/CollectionView/ItemsViewStyles.xaml'
        Templates = @{
          ItemsViewDefaultTemplate = 'local:ItemTemplateContext'
          GroupHeaderTemplate = 'local:GroupTemplateContext'
          CarouselItemsViewDefaultTemplate = 'local:ItemTemplateContext'
        }
      },
      @{
        Path = 'src/Controls/src/Core/Platform/Windows/Styles/ShellStyles.xaml'
        Templates = @{
          ShellFlyoutBaseShellItemTemplate = 'core:NavigationViewItemViewModel'
          ShellFlyoutMenuItemTemplate = 'core:NavigationViewItemViewModel'
          SearchHandlerItemTemplate = 'platform:ItemTemplateContext'
        }
      },
      @{
        Path = 'src/Controls/src/Core/Platform/Windows/TabbedPage/TabbedPageStyle.xaml'
        Templates = @{ TabBarNavigationViewMenuItem = 'core:NavigationViewItemViewModel' }
      },
      @{
        Path = 'src/Controls/src/Core/Compatibility/Handlers/ListView/Windows/ListViewStyles.xaml'
        Templates = @{
          TextCell = 'controls:TextCell'
          ListViewHeaderTextCell = 'controls:TextCell'
          ImageCell = 'controls:ImageCell'
          SwitchCell = 'controls:SwitchCell'
          EntryCell = 'controls:EntryCell'
        }
      },
      @{
        Path = 'src/Controls/src/Core/Compatibility/Handlers/TableView/Windows/TableViewStyles.xaml'
        Templates = @{
          TableRoot = 'controls:TableRoot'
          TableSection = 'controls:TableSection'
        }
      },
      @{
        Path = 'src/Compatibility/Core/src/Windows/ListViewStyles.xaml'
        Templates = @{
          CompatibilityTextCell = 'controls:TextCell'
          CompatibilityListViewHeaderTextCell = 'controls:TextCell'
          CompatibilityImageCell = 'controls:ImageCell'
          CompatibilitySwitchCell = 'controls:SwitchCell'
          CompatibilityEntryCell = 'controls:EntryCell'
        }
      },
      @{
        Path = 'src/Compatibility/Core/src/Windows/PickerStyle.xaml'
        Templates = @{ ComboBoxHeader = 'controls:Picker' }
      },
      @{
        Path = 'src/Compatibility/Core/src/Windows/Resources.xaml'
        Templates = @{
          CompatibilityTextCell = 'controls:TextCell'
          CompatibilityListViewHeaderTextCell = 'controls:TextCell'
          CompatibilityImageCell = 'controls:ImageCell'
          CompatibilitySwitchCell = 'controls:SwitchCell'
          CompatibilityEntryCell = 'controls:EntryCell'
        }
      }
    )

    foreach ($expectation in $expectations) {
      $document = Get-XamlDocument $expectation.Path

      foreach ($templateKey in $expectation.Templates.Keys) {
        $template = Get-XamlDataTemplate $document $templateKey

        $template | Should -Not -BeNullOrEmpty -Because "$($expectation.Path) should define DataTemplate '$templateKey'"
        Get-XamlAttributeValue $template 'DataType' | Should -Be $expectation.Templates[$templateKey] -Because "$($expectation.Path) '$templateKey' should declare the trimmed binding source type"
      }
    }
  }

  It 'keeps PageControl title as a typed XAML binding rather than code-behind reimplementation' {
    foreach ($path in @(
      'src/Controls/src/Core/Platform/Windows/Styles/MauiControlsPageControlStyle.xaml',
      'src/Compatibility/Core/src/Windows/PageControlStyle.xaml'
    )) {
      $document = Get-XamlDocument $path
      $titleTextBlock = Get-XamlElements $document 'TextBlock' |
        Where-Object { (Get-XamlAttributeValue $_ 'Text') -eq '{Binding Title}' } |
        Select-Object -First 1

      $titleTextBlock | Should -Not -BeNullOrEmpty -Because "$path should retain the original title binding"
      Get-XamlAttributeValue $titleTextBlock 'DataType' | Should -Be 'controls:Page'
      Get-XamlAttributeValue $titleTextBlock 'Name' | Should -BeNullOrEmpty
    }
  }

  It 'uses the original data context for CollapseWhenEmpty bindings inside cell and table templates' {
    $expectations = @(
      @{
        Path = 'src/Controls/src/Core/Compatibility/Handlers/ListView/Windows/ListViewStyles.xaml'
        Template = 'TextCell'
        TextVisibility = '{Binding Text, Converter={StaticResource CollapseWhenEmpty}}'
        DetailVisibility = '{Binding Detail, Converter={StaticResource CollapseWhenEmpty}}'
      },
      @{
        Path = 'src/Controls/src/Core/Compatibility/Handlers/ListView/Windows/ListViewStyles.xaml'
        Template = 'ImageCell'
        TextVisibility = '{Binding Text, Converter={StaticResource CollapseWhenEmpty}}'
        DetailVisibility = '{Binding Detail, Converter={StaticResource CollapseWhenEmpty}}'
      },
      @{
        Path = 'src/Compatibility/Core/src/Windows/ListViewStyles.xaml'
        Template = 'CompatibilityTextCell'
        TextVisibility = '{Binding Text, Converter={StaticResource CollapseWhenEmpty}}'
        DetailVisibility = '{Binding Detail, Converter={StaticResource CollapseWhenEmpty}}'
      },
      @{
        Path = 'src/Compatibility/Core/src/Windows/Resources.xaml'
        Template = 'CompatibilityTextCell'
        TextVisibility = '{Binding Text, Converter={StaticResource CollapseWhenEmpty}}'
        DetailVisibility = '{Binding Detail, Converter={StaticResource CollapseWhenEmpty}}'
      }
    )

    foreach ($expectation in $expectations) {
      $document = Get-XamlDocument $expectation.Path
      $template = Get-XamlDataTemplate $document $expectation.Template
      $textBlocks = @(Get-XamlElements $template 'TextBlock' | Where-Object { Get-XamlAttributeValue $_ 'Visibility' })

      (Get-XamlAttributeValue $textBlocks[0] 'Visibility') | Should -Be $expectation.TextVisibility
      (Get-XamlAttributeValue $textBlocks[1] 'Visibility') | Should -Be $expectation.DetailVisibility
    }

    foreach ($expectation in @(
      @{ Path = 'src/Controls/src/Core/Compatibility/Handlers/TableView/Windows/TableViewStyles.xaml'; Template = 'TableRoot' },
      @{ Path = 'src/Controls/src/Core/Compatibility/Handlers/TableView/Windows/TableViewStyles.xaml'; Template = 'TableSection' }
    )) {
      $document = Get-XamlDocument $expectation.Path
      $template = Get-XamlDataTemplate $document $expectation.Template
      $textBlock = Get-XamlElements $template 'TextBlock' | Select-Object -First 1

      Get-XamlAttributeValue $textBlock 'Visibility' | Should -Be '{Binding Title,Converter={StaticResource CollapseWhenEmpty}}'
    }
  }

  It 'does not replace Binding with TemplateBinding where TemplateBinding type conversion is not equivalent' {
    $slider = Get-XamlDocument 'src/Core/src/Platform/Windows/Styles/MauiSliderStyle.xaml'
    $sliderImage = Get-XamlElements $slider 'Image' | Where-Object { (Get-XamlAttributeValue $_ 'Name') -eq 'ThumbImage' } | Select-Object -First 1
    Get-XamlAttributeValue $sliderImage 'Source' | Should -Be '{Binding Tag, RelativeSource={RelativeSource TemplatedParent}}'

    $legacySlider = Get-XamlDocument 'src/Compatibility/Core/src/Windows/SliderStyle.xaml'
    $legacySliderImage = Get-XamlElements $legacySlider 'Image' | Where-Object { (Get-XamlAttributeValue $_ 'Name') -eq 'ThumbImage' } | Select-Object -First 1
    $legacyThumb = Get-XamlElements $legacySlider 'Thumb' |
      Where-Object { (Get-XamlAttributeValue $_ 'Name') -eq 'HorizontalImageThumb' } |
      Select-Object -First 1
    Get-XamlAttributeValue $legacySliderImage 'Source' | Should -Be '{Binding Tag, RelativeSource={RelativeSource TemplatedParent}}'
    Get-XamlAttributeValue $legacyThumb 'Tag' | Should -Be '{Binding ThumbImageSource, RelativeSource={RelativeSource TemplatedParent}}'

    $checkBox = Get-XamlDocument 'src/Compatibility/Core/src/Windows/FormsCheckBoxStyle.xaml'
    $controlTemplate = Get-XamlElements $checkBox 'ControlTemplate' | Select-Object -First 1
    Get-XamlAttributeValue $controlTemplate 'TargetType' | Should -Be 'CheckBox'

    $normalRectangle = Get-XamlElements $checkBox 'Rectangle' |
      Where-Object { (Get-XamlAttributeValue $_ 'Name') -eq 'NormalRectangle' } |
      Select-Object -First 1
    Get-XamlAttributeValue $normalRectangle 'Stroke' | Should -Be '{Binding TintBrush, RelativeSource={RelativeSource TemplatedParent}}'
  }
}
