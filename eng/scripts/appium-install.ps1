$appiumVersion = '2.0.0-beta.64'
$windowsDriverVersion = 2.7.2
$androidDriverVersion = 2.25.1
$iOSDriverVersion = 4.30.2
$macDriverVersion = 1.6.1

node -v
npm install -g npm
node -v
npm install -g appium@$appiumVersion
appium -v
$x = appium driver list --installed --json  | ConvertFrom-Json

if(!$x.windows)
{
    appium driver install --source=npm appium-windows-driver@$windowsDriverVersion
}
else
{
    appium driver uninstall windows
    appium driver install --source=npm appium-windows-driver@$windowsDriverVersion
}

if(!$x.uiautomator2)
{
    appium driver install uiautomator2@$androidDriverVersion
}
else
{
    appium driver uninstall uiautomator2
    appium driver install uiautomator2@$androidDriverVersion
}

if(!$x.xcuitest)
{
    appium driver install xcuitest@$iOSDriverVersion
}
else
{
    appium driver uninstall xcuitest
    appium driver install xcuitest@$iOSDriverVersion
}

if(!$x.mac2)
{
    appium driver install mac2@$macDriverVersion
}
else
{
    appium driver uninstall mac2
    appium driver install mac2@$macDriverVersion
}