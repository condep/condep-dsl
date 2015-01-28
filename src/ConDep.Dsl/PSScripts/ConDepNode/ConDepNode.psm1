Set-StrictMode -Version 3

function Get-ConDepNodeState([string]$path, $hash) {
	$path = $ExecutionContext.InvokeCommand.ExpandString($path)

    $conDepReturnValues = New-Object PSObject -Property @{         
        ConDepResult    = New-Object PSObject -Property @{
			IsNodeServiceRunning = $false
			NeedNodeDeployment = $false
        }                 
    }                     

    if(Test-Path $path) {
		$localHash = Get-ConDepFileHash $path
		write-verbose "Node Hash Client : $hash"
		write-verbose "Node Hash Server : $localHash"

		if($localHash -ne $hash) {
			$conDepReturnValues.ConDepResult.NeedNodeDeployment = $true
			return $conDepReturnValues
		}

		add-type -AssemblyName System.ServiceProcess
		$wmiService = Get-WmiObject -Class Win32_Service -Filter "Name='condepnode'"

		if($wmiService) {
			$conDepReturnValues.ConDepResult.IsNodeServiceRunning = ($wmiService.State -eq "Running")
			return $conDepReturnValues
		}
    }

    $conDepReturnValues.ConDepResult.NeedNodeDeployment = $true
    return $conDepReturnValues    
}

function New-ConDepNodeService([string]$path, $url) {
    write-host 'Creating ConDepNode Windows service'
    New-Service -Name ConDepNode -BinaryPathName "$path $url" -StartupType Manual
}

function Start-ConDepNode() {
	add-type -AssemblyName System.ServiceProcess
	$wmiService = Get-WmiObject -Class Win32_Service -Filter "Name='condepnode'"

	if($wmiService) {
		if($wmiService.State -eq "Stopped") {
    		$service = Get-Service condepnode -ErrorAction Stop
			$service.Start()
			$service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
		}
		else {
			throw "Failed to start ConDepNode Windows service, because it's not stopped. The current state of the service is $($wmiService.State)."
		}
	}
	else {
		throw "ConDepNode Windows service does not exist, and therefore cannot be started."
	}
}

function Stop-ConDepNode() {
	add-type -AssemblyName System.ServiceProcess
	$wmiService = Get-WmiObject -Class Win32_Service -Filter "Name='condepnode'"

	if($wmiService) {
		if($wmiService.State -eq "Running") {
    		$service = Get-Service condepnode -ErrorAction Stop
			$service.Stop()
			$service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped)
		}
		else {
			throw "Failed to stop ConDepNode Windows service, because it's not running. The current state of the service is $($wmiService.State)."
		}
	}
	else {
		throw "ConDepNode Windows service does not exist, and therefore cannot be stopped."
	}
}

function Remove-ConDepNodeService() {
	add-type -AssemblyName System.ServiceProcess
	$wmiService = Get-WmiObject -Class Win32_Service -Filter "Name='condepnode'"

	if($wmiService) {
		if($wmiService.State -eq "Running") {
			Stop-ConDepNode 
		}

        $deleteResult = $wmiService.Delete()
        if($deleteResult.ReturnValue -ne 0) {
            throw "Failed to delete ConDepNode service. Return code was $($deleteResult.ReturnValue)"
        }
	}
}

function Add-ConDepNode([string]$path, $data, $url, $port) {
    $path = $ExecutionContext.InvokeCommand.ExpandString($path)

    $dir = Split-Path $path

	Remove-ConDepNodeService
    
	write-host "Deploying node to $dir"
    $dirInfo = [IO.Directory]::CreateDirectory($dir)

	if(Test-Path $path) {
		[IO.File]::Delete($path)
	}

    [IO.FileStream]$filestream = [IO.File]::OpenWrite( $path )
    $filestream.Write( $data, 0, $data.Length )
    $filestream.Close()

    write-host "Adding firewall rule for ConDepNode on port $port"
    netsh advfirewall firewall delete rule name='ConDepNode'
    netsh advfirewall firewall add rule name='ConDepNode' protocol=TCP localport=$port action=allow dir=IN

	New-ConDepNodeService $path $url
	Start-ConDepNode
}

function Get-ConDepFileDiffs($json, $dir) {
	$fileHashes = $json | ConvertFrom-Json

	$returnValues = @()

	foreach($file in $fileHashes) {
		$hash = GetHash (Join-Path -path $dir -childpath $($file.FileName))
		$returnValues += New-Object psobject -Property @{
			FileName = $file.FileName
			IsEqual = ($hash -eq $file.Hash) 
		}
	}
	return ($returnValues | ConvertTo-Json)
}

function Get-ConDepFileHash($path) {
    if(Test-Path $path) {
        $md5 = [System.Security.Cryptography.MD5]::Create()
        $hash = [System.BitConverter]::ToString($md5.ComputeHash([System.IO.File]::ReadAllBytes($path)))
        return $hash.Replace("-", "")
    }
    else {
        return ""
    }
}

Export-ModuleMember -Function *-*
