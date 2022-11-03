# Ensure unsigned powershell script execution is allowed: Set-ExecutionPolicy -ExecutionPolicy RemoteSigned

param (
	[Parameter(Mandatory=$false)] [String] $version,
    [Parameter(Mandatory=$false)] [String] $publshFolder,
	[Parameter(Mandatory=$false)] [String] $nugetUrl, 
	[Parameter(Mandatory=$false)] [String] $apiKey,
    [Parameter(Mandatory=$false)] [String] $openApiJson
)

. $PSScriptRoot/_core.ps1

# Setup
$workingDirectory = [System.IO.Path]::Combine("$PSScriptRoot", "..", "..")
$localOpenApiJson = [System.IO.Path]::Combine($workingDirectory, "keycloak.openapi.json")
$localOpenApiJsonCleaned = [System.IO.Path]::Combine($workingDirectory, "keycloak.openapi.clean.json")
Push-Location $workingDirectory

# Set input file
if (!$openApiJson){
    $openApiJson = "https://raw.githubusercontent.com/dahag-ag/keycloak-openapi/main/OpenApiDefinitions/keycloak-19.0.0.json"
}

# NPM install
Npm-Restore Build/openapi-generator

# Download or copy file
if ($openApiJson.StartsWith("http")) {
	Invoke-WebRequest -Uri $openApiJson -OutFile $localOpenApiJson
} else {
	Copy-Item $openApiJson $localOpenApiJson
}

# Clean up downloaded file (replace '<' and '>' in descriptions)
$content = [System.IO.File]::ReadAllText($localOpenApiJson)
$content = $content -replace '("?description"?\s*:\s*)(.*)<(.*)>(.*)', '$1$2{$3}$4'
[System.IO.File]::WriteAllText($localOpenApiJsonCleaned, $content)

# Add humanized C# operation name
& node Build/openapi-generator/openapi-generator-add-charp-action.js $localOpenApiJsonCleaned $localOpenApiJsonCleaned

# Generate client
Push-Location Build/openapi-generator
& node_modules/.bin/openapi-generator-cli generate -c openapi-generator.config.json -g csharp-netcore --template-dir templates --type-mappings date-span='TimeSpan' -i $localOpenApiJsonCleaned -o ../../FS.Keycloak.RestApiClient
Pop-Location

# Fix smaller issues from open API generator
& node Build/openapi-generator/openapi-generator-fix-shortcoming.js FS.Keycloak.RestApiClient

Build-Project -project FS.Keycloak.RestApiClient -version $version
Publish-Nuget -project FS.Keycloak.RestApiClient/src/FS.Keycloak.RestApiClient -version $version -publshFolder $publshFolder
Push-Nuget -project -publshFolder $publshFolder -serverUrl $nugetUrl -apiKey $apiKey

Pop-Location