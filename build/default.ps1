properties {
	$pwd = Split-Path $psake.build_script_file	
	$build_directory  = "$pwd\output\condep-dsl"
	$configuration = "Release"
	$preString = "-beta"
	$releaseNotes = ""
	$nunitPath = "$pwd\..\src\packages\NUnit.Runners.2.6.3\tools"
	$nuget = "$pwd\..\tools\nuget.exe"
}
 
include .\..\tools\psake_ext.ps1

Framework '4.6x64'

function GetNugetAssemblyVersion($assemblyPath) {
	$versionInfo = Get-Item $assemblyPath | % versioninfo

	return "$($versionInfo.FileMajorPart).$($versionInfo.FileMinorPart).$($versionInfo.FileBuildPart)$preString"
}

task default -depends Build-All, Test-All, Pack-All
task ci -depends Build-All, Pack-All

task Build-All -depends Clean, ResotreNugetPackages, Build, Create-BuildSpec-ConDep-Dsl
task Test-All -depends Test
task Pack-All -depends Pack-ConDep-Dsl

task ResotreNugetPackages {
	Exec { & $nuget restore "$pwd\..\src\condep-dsl.sln" }
}

task Build {
	Exec { msbuild "$pwd\..\src\condep-dsl.sln" /t:Build /p:Configuration=$configuration /p:OutDir=$build_directory /p:GenerateProjectSpecificOutputFolder=true}
}

task Test {
	Exec { & $nunitPath\nunit-console.exe $build_directory\ConDep.Dsl.Tests\ConDep.Dsl.Tests.dll /nologo /nodots /xml=$build_directory\TestResult.xml }
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
		-licenseUrl "http://www.con-dep.net/license/" `
		-projectUrl "http://www.con-dep.net/" `
		-description "Note: This package is for extending the ConDep DSL. If you're looking for ConDep to do deployment or infrastructure as code, please use the ConDep package. ConDep is a highly extendable Domain Specific Language for Continuous Deployment, Continuous Delivery and Infrastructure as Code on Windows." `
		-iconUrl "https://raw.github.com/condep/ConDep/master/images/ConDepNugetLogo.png" `
		-releaseNotes "$releaseNotes" `
		-tags "Continuous Deployment Delivery Infrastructure WebDeploy Deploy msdeploy IIS automation powershell remote aws azure" `
		-dependencies @(
			@{ Name="log4net"; Version="[2.0.0]"},
			@{ Name="Newtonsoft.Json"; Version="[6.0.6,7)"},
			@{ Name="SlowCheetah.Tasks.Unofficial"; Version="[1.0.0]"},
			@{ Name="Microsoft.AspNet.WebApi.Client"; Version="[4.0.30506]"}
		) `
		-files @(
			@{ Path="ConDep.Dsl\ConDep.Dsl.dll"; Target="lib/net40"}, 
			@{ Path="ConDep.Dsl\ConDep.Dsl.xml"; Target="lib/net40"}
		)
}

task Pack-ConDep-Dsl {
	Exec { & $nuget pack "$build_directory\condep.dsl.nuspec" -OutputDirectory "$build_directory" }
}