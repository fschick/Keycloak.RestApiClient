# Ensure unsigned powershell script execution ist allowed: Set-ExecutionPolicy -ExecutionPolicy RemoteSigned

#
#region Build
#

function Npm-Restore([String] $folder) {
	Push-Location $folder
	
	# Restore npm packages
	& npm ci --prefer-offline --no-audit
	if(!$?) {
		Pop-Location
		exit $LASTEXITCODE
	}
	
	Pop-Location
}

function Build-Project([String] $project, [String] $version = "0.0.0") {
    # Configure
    $fileVersion = Get-FileVersion -version $version

    # Create arguments
    $arguments = @("-p:Version=$version", "-p:FileVersion=$fileVersion")

    # Build with configuration "Debug"
    Write-Host -ForegroundColor Green "dotnet build $project --configuration Debug $arguments"
    & dotnet build $project --configuration Debug $arguments
    if(!$?) {
        exit $LASTEXITCODE
    }

    # Build with configuration "Release"
    $arguments += "-warnaserror"
    Write-Host -ForegroundColor Green "dotnet build $project --configuration Release $arguments"
    & dotnet build $project --configuration Release $arguments
    if(!$?) {
        exit $LASTEXITCODE
    }
}

#endregion

#
#region Tests
#

function Test-Project([String] $project, [String] $version, [String] $filter) {
    # Configure
    $fileVersion = Get-FileVersion -version $version

    # Create arguments
    $arguments = @("--logger", "trx", "-p:Version=$version", "-p:FileVersion=$fileVersion")

    if ($filter) {
        $arguments +=@("--filter", $filter)
    }

    # Run tests
    Write-Host -ForegroundColor Green "dotnet test $project $arguments"
    & dotnet test $project $arguments
    if(!$?) {
        exit $LASTEXITCODE
    }
}

#endregion

#
#region Publish
#

function Publish-Project([String] $project, [String] $version, [String] $framework, [String] $runtime, [String] $publshFolder, [String] $publishProfile, [String] $password) {
    # Configure
    $fileVersion = Get-FileVersion -version $version
    $configuration = "Release"

    # Create arguments
    $arguments = @("--configuration", $configuration, "--framework", $framework, "-p:Version=$version", "-p:FileVersion=$fileVersion")

    $isSelfContained = !!$runtime
    if ($isSelfContained) {
        $arguments +=@("--self-contained", "--runtime", $runtime)
    }

    if ($publshFolder) {
        $arguments +=@("--output", $publshFolder)
    }
	else {
		$projectIsFile = Test-Path $project -PathType Leaf
		$projectFolder = $(if($projectIsFile) {Split-Path -Path $project} else {$project})
        $runtimeFolder = $(if($isSelfContained) {"/$runtime"} else {""})
        $publshFolder =  "$projectFolder/bin/$configuration/$framework$runtimeFolder/publish"
	}

    if ($publishProfile) {
        $arguments +=@("-p:PublishProfile=""$publishProfile""")
    }

    if ($password) {
        $arguments +=@("-p:Password=""$password""")
    }

    # Publish project
    Write-Host -ForegroundColor Green "dotnet publish $project $arguments"
    & dotnet publish $project $arguments
    if(!$?) {
        exit $LASTEXITCODE
    }
}

function Publish-Nuget([String] $project, [String] $version, [String] $publshFolder) {
    # Configure
    $fileVersion = Get-FileVersion -version $version
    $configuration = "Release"

    # Create arguments
    $arguments = @("--configuration", $configuration, "-p:Version=$version", "-p:FileVersion=$fileVersion")

    if ($publshFolder) {
        $arguments +=@("--output", $publshFolder)
    }

    Write-Host -ForegroundColor Green "dotnet pack $project $arguments"
    & dotnet pack $project $arguments
}

function Push-Nuget([String] $publshFolder, [String] $serverUrl, [String] $apiKey) {
    # Create arguments
    $arguments = @()

    if ($serverUrl) {
        $arguments += @("--source", $serverUrl);
    }

    if ($apiKey) {
        $arguments += @("--api-key", $apiKey);
    }

    Write-Host -ForegroundColor Green "dotnet nuget push $publshFolder/*.nupkg $arguments"
    & dotnet nuget push $publshFolder/*.nupkg $arguments
}

#endregion

function Get-FileVersion([String] $version) {
    return $version -replace '(\d+(?:\.\d+)*)(.*)', '$1'
}