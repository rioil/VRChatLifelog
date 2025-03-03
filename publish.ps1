Param(
  [parameter(Mandatory = $true)][string]$version
)

$publishDir = "./publish/"
$runtime = "win-x64"
$srcDir = "./VRChatLifelog/"
$publishName = "VRChatLifelog-$version-$runtime"

# clean
Write-Output "Cleaning the publish directory..."
Remove-Item -Recurse -Force ${publishDir}${runtime}/

# build
Write-Output "Building the project..."
dotnet publish $srcDir -o ${publishDir}${runtime}/ --runtime $runtime -p:PublishSingleFile=true -p:UseAppHost=true --no-self-contained

# delete pdb files
Write-Output "Deleting pdb files..."
Remove-Item ${publishDir}${runtime}/*.pdb

# generate SBOM
Write-Output "Generating SBOM..."
sbom-tool generate -ps rioil -pn VRChatLifelog -pv $version -b ${publishDir}${runtime}/ -bc $srcDir -D true
Copy-Item ${publishDir}${runtime}/_manifest/spdx_2.2/manifest.spdx.json ${publishDir}${publishName}.spdx.json
Copy-Item ${publishDir}${runtime}/_manifest/spdx_2.2/manifest.spdx.json.sha256 ${publishDir}${publishName}.spdx.json.sha256

# zip
Write-Output "Compress to a zip file..."
$prevProgressPreference = $global:ProgressPreference
# Suppress progress bar because it causes an error when running in Developer PowerShell of Visual Studio
$global:ProgressPreference = "SilentlyContinue"
Compress-Archive -Force -Path ${publishDir}${runtime}/*.* -DestinationPath ${publishDir}${publishName}.zip
$global:ProgressPreference = $prevProgressPreference

Write-Output "Successfully published version $version to $publishDir!"
