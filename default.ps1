Framework '4.6'
Include scripts\Build-Nuget.ps1

properties {
  $rootDir = Resolve-Path .
  $buildDir = Join-Path $rootDir "Build"
  $srcDir = Join-Path $rootDir "src"
  $solutionPath = Join-Path $rootDir "MvcBundlers.sln"
  $buildConfiguration = "Release"
  $packagesDir = Join-Path $rootDir "Packages"
  $nugetDir = Join-Path $rootDir ".nuget"
  $nugetRepo = if ($env:nugetRepo -eq $null) { throw "You must set the NugetRepo environment variable." } else { $env:nugetRepo }
  $nuspecsDir = Join-Path $rootDir "nuspecs"
}

task ? -description "Helper to display task info" {
    Write-Documentation
}

task default -depends Compile

task Compile -depends Download-Packages, Clean {
    Write-Host "Building a $buildConfiguration build"
	exec { msbuild /v:m $solutionPath /p:"Configuration=$buildConfiguration;Platform=Any CPU;TrackFileAccess=false" }
}

task Clean {
	if ((Test-Path $buildDir)) {
		rm -Recurse -Force $buildDir | Out-Null
	}
	mkdir $buildDir | Out-Null
}
