properties {
	$nuget = Join-Path $nugetDir "nuget.exe"
}

task Download-Packages -description "If there are no packages they will be downloaded.  To force an update use Update-Packages" {
	Sync-ProjectPackages
}

task Update-Packages -depends Clean-Packages -description "Forces an update of nuget packages." {
  Sync-ProjectPackages
}

task Clean-Packages {
  rm $packagesDir -Recurse -Force
}

task Publish-Package -depends Create-Package {
	copy "$buildDir\*.nupkg" $nugetRepo
}

task Create-Package -depends Compile {
  $nuspecs = ls $nuspecsDir *.nuspec
  $version = Get-Version
  foreach ($spec in $nuspecs) {
	    . $nuget pack $spec.FullName -outputDirector $buildDir -version $version -symbols
  }
}

function Sync-ProjectPackages {
  if ((Test-Path $packagesDir) -eq $false) {
	$env:EnableNuGetPackageRestore = "true"
    Get-ChildItem $srcDir -Recurse -Include packages.config |
    ForEach-Object { 
      "Installing packages for $_"
      . $nuget install $_.FullName -OutputDirectory $packagesDir -NoCache -Source $nugetRepo
    }
  }
}

function Get-Version {
  if ($env:BuildVersion) { $version = $env:BuildVersion }
  else { throw '$env:BuildVersion not set.' }
  return $version
}
