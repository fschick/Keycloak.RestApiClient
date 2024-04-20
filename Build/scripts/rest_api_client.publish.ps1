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
$localOpenApiJsonFixed = [System.IO.Path]::Combine($workingDirectory, "keycloak.openapi.fixed.json")
$localOpenApiJsonOperation = [System.IO.Path]::Combine($workingDirectory, "keycloak.openapi.operation.json")

Push-Location $workingDirectory

# Set input file
if (!$openApiJson){
    $openApiJson = "https://www.keycloak.org/docs-api/latest/rest-api/openapi.json"
}

# NPM install
Npm-Restore Build/openapi-generator

# Download or copy file
if ($openApiJson.StartsWith("http")) {
	Invoke-WebRequest -Uri $openApiJson -OutFile $localOpenApiJson
} else {
	Copy-Item $openApiJson $localOpenApiJson
}

# Add humanized C# operation name
& node Build/openapi-generator/openapi-generator-fix-spec.js $localOpenApiJson $localOpenApiJsonFixed
& node Build/openapi-generator/openapi-generator-add-charp-action.js $localOpenApiJsonFixed $localOpenApiJsonOperation

# Generate client
Push-Location Build/openapi-generator
& node_modules/.bin/openapi-generator-cli generate -c openapi-generator.config.json -g csharp --template-dir templates --type-mappings date-span='TimeSpan' --global-property apiTests=false -o ../../FS.Keycloak.RestApiClient -i $localOpenApiJsonOperation
Pop-Location

# Fix smaller issues from open API generator
& node Build/openapi-generator/openapi-generator-fix-project.js FS.Keycloak.RestApiClient

# Copy README.md
Copy-Item README.md FS.Keycloak.RestApiClient/README.md

# Build and publish NuGet package
Build-Project -project FS.Keycloak.RestApiClient -version $version
Publish-Nuget -project FS.Keycloak.RestApiClient/src/FS.Keycloak.RestApiClient -version $version -publshFolder $publshFolder
Push-Nuget -project -publshFolder $publshFolder -serverUrl $nugetUrl -apiKey $apiKey

Pop-Location