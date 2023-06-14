node -v
npm install -g npm
node -v
npm install -g appium@2.0.0-beta.61
appium -v
$x = appium driver list --installed --json  | ConvertFrom-Json
if(!$x.windows)
{
    appium driver install --source=npm appium-windows-driver
}

if(!$x.uiautomator2)
{
    appium driver install uiautomator2
}

if(!$x.xcuitest)
{
    appium driver install xcuitest
}

if(!$x.mac2)
{
    appium driver install mac2
}