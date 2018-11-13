param(
    [string]$branch = "master",
    [string]$token
)

$docsUri = "https://$token@github.com/xamarin/Xamarin.Forms-api-docs.git"

if($branch.StartsWith("refs/pull")) {
    $branch = "master"
} else {

    $docBranches = git ls-remote --heads $docsUri
    $matched = $false
    foreach ($db in $docBranches) {

        if($db.EndsWith($branch)) {
            # $branch corresponds to an actual branch in docs, so use it
            $matched = $true
            break;
        }
    }

    if(-not $matched) {
        $branch = "master"
    }

    $branch = $branch.Replace("refs/heads/", "")
}

# API docs maintains its copy of master in `eng-master`
if ($branch -eq "master") {$branch = "eng-master"}

pushd ..\..

mkdir docstemp
pushd docstemp

git clone -b $branch --single-branch $docsUri

pushd .\Xamarin.Forms-api-docs

$mdoc = '..\..\tools\mdoc\mdoc.exe'

& $mdoc export-msxdoc .\docs

mv Xamarin.Forms.*.xml ..\..\docs -Force

popd
popd

del docstemp -R -Force

popd