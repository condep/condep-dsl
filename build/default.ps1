properties {
	$pwd = Split-Path $psake.build_script_file	
	$build_directory  = "$pwd\output\condep-dsl"
	$configuration = "Release"
	$releaseNotes = ""
	$nunitPath = "$pwd\..\src\packages\NUnit.Runners.2.6.3\tools"
	$nuget = "$pwd\..\tools\nuget.exe"
}
 
include .\..\tools\psake_ext.ps1

Framework '4.6x64'

function GetNugetAssemblyVersion($assemblyPath) {
    
    if(Test-Path Env:\APPVEYOR_BUILD_VERSION)
    {
        $appVeyorBuildVersion = $env:APPVEYOR_BUILD_VERSION
     
		# Getting the version number. Without the beta part, if its a beta package   
        $version = $appVeyorBuildVersion.Split('.')
        $major = $version[0] 
        $minor = $version[1] 
        $patch = $version[2].Split('-') | Select-Object -First 1

        # Setting beta postfix, if beta build. The beta number must be 5 digits, therefor this operation.
        $betaString = ""
        if($appVeyorBuildVersion.Contains("beta"))
        {
        	$buildNumber = $appVeyorBuildVersion.Split('-') | Select-Object -Last 1 | % {$_.replace("beta","")}
        	switch ($buildNumber.length) 
        	{	 
            	1 {$buildNumber = $buildNumber.Insert(0, '0').Insert(0, '0').Insert(0, '0').Insert(0, '0')} 
            	2 {$buildNumber = $buildNumber.Insert(0, '0').Insert(0, '0').Insert(0, '0')} 
            	3 {$buildNumber = $buildNumber.Insert(0, '0').Insert(0, '0')}
            	4 {$buildNumber = $buildNumber.Insert(0, '0')}                
            	default {$buildNumber = $buildNumber}
        	}
        	$betaString = "-beta$buildNumber" 
        }	
        return "$major.$minor.$patch$betaString"
    }
    else
    {
		#When building on local machine, set versionnumber from assembly info.
        $versionInfo = Get-Item $assemblyPath | % versioninfo
        return "$($versionInfo.FileVersion)"
    }
}

task default -depends Build-All, Test-All, Pack-All
task ci -depends Build-All, Pack-All

task Build-All -depends Clean, ResotreNugetPackages, Build, Check-VersionExists, Create-BuildSpec-ConDep-Dsl
task Test-All -depends Test
task Pack-All -depends Pack-ConDep-Dsl

task Check-VersionExists {
	$version = $(GetNugetAssemblyVersion $build_directory\ConDep.Dsl\ConDep.Dsl.dll) 
	Exec { 
		$packages = & $nuget list "ConDep.Dsl" -source "https://www.myget.org/F/condep/api/v3/index.json" -prerelease -allversions
		ForEach($package in $packages){
			$packageName = $package.Split(' ') | Select-Object -First 1
			if($packageName -eq "ConDep.Dsl"){
				$packageVersionNumber = $package.Split(' ') | Select-Object -Last 1
				if($packageVersionNumber -eq $version){
					throw "ConDep.Dsl $packageVersionNumber already exists on myget. Have you forgot to update version in appveyor.yml?"
				}
			}
		}
	}
}

task ResotreNugetPackages {
	Exec { & $nuget restore "$pwd\..\src\condep-dsl.sln" }
}

task Build {
	Exec { msbuild "$pwd\..\src\condep-dsl.sln" /t:Build /p:Configuration=$configuration /p:OutDir=$build_directory /p:GenerateProjectSpecificOutputFolder=true}
}

task Test {
	Exec { & $nunitPath\nunit-console.exe $build_directory\ConDep.Dsl.Tests\ConDep.Dsl.Tests.dll /nologo /nodots /xml=$build_directory\TestResult.xml $exclude}
}

task Clean {
	Write-Host "Cleaning Build output"  -ForegroundColor Green
	Remove-Item $build_directory -Force -Recurse -ErrorAction SilentlyContinue
}

task Create-BuildSpec-ConDep-Dsl {
	Generate-Nuspec-File `
		-file "$build_directory\condep.dsl.nuspec" `
		-version $(GetNugetAssemblyVersion $build_directory\ConDep.Dsl\ConDep.Dsl.dll) `
		-id "ConDep.Dsl" `
		-title "ConDep.Dsl" `
		-licenseUrl "http://www.condep.io/license/" `
		-projectUrl "http://www.condep.io/" `
		-description "Note: This package is for extending the ConDep DSL. If you're looking for ConDep to do deployment or infrastructure as code, please use the ConDep package. ConDep is a highly extendable Domain Specific Language for Continuous Deployment, Continuous Delivery and Infrastructure as Code on Windows." `
		-iconUrl "https://raw.github.com/condep/ConDep/master/images/ConDepNugetLogo.png" `
		-releaseNotes "$releaseNotes" `
		-tags "Continuous Deployment Delivery Infrastructure WebDeploy Deploy msdeploy IIS automation powershell remote aws azure" `
		-dependencies @(
			@{ Name="log4net"; Version="[2.0.0]"},
			@{ Name="Newtonsoft.Json"; Version="[6.0.6,7)"},
			@{ Name="SlowCheetah.Tasks.Unofficial"; Version="[1.0.0]"},
			@{ Name="Microsoft.AspNet.WebApi.Client"; Version="[5,6)"}
		) `
		-files @(
			@{ Path="ConDep.Dsl\ConDep.Dsl.dll"; Target="lib/net45"}, 
			@{ Path="ConDep.Dsl\ConDep.Dsl.xml"; Target="lib/net45"}
		)
}

task Pack-ConDep-Dsl {
	Exec { & $nuget pack "$build_directory\condep.dsl.nuspec" -OutputDirectory "$build_directory" }
}