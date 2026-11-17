#Requires -Modules Pester

Describe 'Windows XAML binding behavior' {
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

  It 'keeps PageControl title as a XAML binding rather than code-behind reimplementation' {
    foreach ($path in @(
      'src/Controls/src/Core/Platform/Windows/Styles/MauiControlsPageControlStyle.xaml'
    )) {
      $document = Get-XamlDocument $path
      $titleTextBlock = Get-XamlElements $document 'TextBlock' |
        Where-Object { (Get-XamlAttributeValue $_ 'Text') -eq '{Binding Title}' } |
        Select-Object -First 1

      $titleTextBlock | Should -Not -BeNullOrEmpty -Because "$path should retain the original title binding"
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
  }
}
