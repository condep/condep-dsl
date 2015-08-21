Set-StrictMode -Version 3

function ConDep-ChocoExist() {
    if (ConDep-ChocoPath) { return $true }
    return $false
}

function ConDep-ChocoPath {
    if($env:ChocolateyInstall) {
        return $env:ChocolateyInstall
    }
    elseif (Test-Path $env:ProgramData\chocolatey) {
            return "$env:ProgramData\chocolatey"
    }
    elseif (Test-Path $env:HOMEDRIVE\chocolatey) {
        return "$env:HOMEDRIVE\chocolatey"
    }
    elseif (Test-Path $env:ALLUSERSPROFILE\chocolatey) {
        return "$env:ALLUSERSPROFILE\chocolatey"
    }
    return $null
}

function ConDep-ChocoExe {
	if(ConDep-ChocoPath) {
		return "$(ConDep-ChocoPath)\bin\choco.exe"
	}
	return "choco.exe"
}

function ConDep-ChocoNew {
	$answer = &(ConDep-ChocoExe) -v

	foreach($line in $answer -split "`n") {
		if(!$line) { next }
		if($line -match "Please run chocolatey.*") { return $false}
		else { return $true }
	}
}

function ConDep-ChocoUpgrade {
	if(ConDep-ChocoNew) {
		write-Host "Upgrading Chocolatey..."
		&(ConDep-ChocoExe) upgrade chocolatey -dvy
	}
	else {
		write-Host "No recent version of Chocolatey found. Installing now..."
		iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1')) | Out-Null 
	}
}

function ConDep-ChocoInstallPackage {

}

function ConDep-ChocoUpgradePackage {

}

function ConDep-ChocoPackageExist {

}

function ConDep-ChocoInstall {
	iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))
}

Get-ChildItem -Path $PSScriptRoot\*.ps1 | Foreach-Object{ . $_.FullName }
Export-ModuleMember -Function *-*
