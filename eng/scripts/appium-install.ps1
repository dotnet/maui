node -v
npm install -g npm
node -v
npm install -g appium@2.0.0-beta.61
appium -v
$x = appium driver list --installed --json  | ConvertFrom-Json
if(!$x.windows)
{
    appium driver install --source=npm appium-windows-driver@2.7.2
}
else
{
    appium driver uninstall windows
    appium driver install --source=npm appium-windows-driver@2.7.2
}

if(!$x.uiautomator2)
{
    appium driver install uiautomator2@2.25.1
}
else
{
    appium driver uninstall uiautomator2
    appium driver install uiautomator2@2.25.1
}

if(!$x.xcuitest)
{
    appium driver install xcuitest@4.30.2
}
else
{
    appium driver uninstall xcuitest
    appium driver install xcuitest@4.30.2
}

if(!$x.mac2)
{
    appium driver install mac2@1.5.3
}
else
{
    appium driver uninstall mac2
    appium driver install mac2@1.5.3
}