function Get-ConDepNode([string]$remFile, $deplNodeVersion) {
	$remFile = $ExecutionContext.InvokeCommand.ExpandString($remFile)
    $nodeVersion = ""

    $conDepReturnValues = New-Object PSObject -Property @{         
        ConDepResult    = New-Object PSObject -Property @{
			NeedNodeDeployment = $true
			IsNodeServiceRunning = $false
        }                 
    }                     

    if(Test-Path $remFile) {
        Write-Host "ConDepNode exists on server"

        $versionInfo = Get-Item $remFile | % versioninfo
        if($versionInfo) {
            $nodeVersion = "$($versionInfo.FileMajorPart).$($versionInfo.FileMinorPart).$($versionInfo.FileBuildPart)"
            Write-Host "Node server version: $nodeVersion"
            Write-Host "Node client version: $deplNodeVersion"

            if($nodeVersion -eq $deplNodeVersion) {
                Write-Host "Node version is same on client and server. No need to deploy."
                $conDepReturnValues.ConDepResult.NeedNodeDeployment = $false
            }
            else {
                Write-Host "Node version is different from client. Will need to deploy ConDepNode."
                $conDepReturnValues.ConDepResult.NeedNodeDeployment = $true
            }
        }

		add-type -AssemblyName System.ServiceProcess
		$wmiService = Get-WmiObject -Class Win32_Service -Filter "Name='condepnode'"

		if($wmiService) {
			$conDepReturnValues.ConDepResult.IsNodeServiceRunning = $true
		}
		else {
			write-host 'ConDepNode service not found. Will need to deploy.'
            $conDepReturnValues.ConDepResult.NeedNodeDeployment = $true
		}
    }

    return $conDepReturnValues    
}

function Start-ConDepNode([string]$remFile, $nodeUrl) {
	write-host 'Checking state of ConDepNode service...'
	add-type -AssemblyName System.ServiceProcess
	$wmiService = Get-WmiObject -Class Win32_Service -Filter "Name='condepnode'"

	if($wmiService) {
		write-host "ConDepNode state is $($wmiService.State)"
		if($wmiService.State -eq "Stopped") {{
			write-host 'ConDepNode is stopped. Starting now...'
    		$service = Get-Service condepnode -ErrorAction Stop
			$service.Start()
			$service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
		}}
		write-host 'ConDepNode started'
	}
	else {
		write-host 'Creating new ConDepNode...'
		New-Service -Name ConDepNode -BinaryPathName "$remFile $nodeUrl" -StartupType Manual
		write-host 'ConDepNode created'
		write-host 'Starting ConDepNode...'
		$service = get-service condepnode
		$service.Start()
		write-host 'Waiting for node to start...'
		$service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
		write-host 'ConDepNode started'

	}
}

function Add-ConDepNode([string]$remFile, $data, $bytes, $nodeUrl) {
    $remFile = $ExecutionContext.InvokeCommand.ExpandString($remFile)

    $dir = Split-Path $remFile

    $dirInfo = [IO.Directory]::CreateDirectory($dir)
    [IO.FileStream]$filestream = [IO.File]::OpenWrite( $remFile )
    $filestream.Write( $data, 0, $bytes )
    $filestream.Close()

    add-type -AssemblyName System.ServiceProcess
    $wmiService = Get-WmiObject -Class Win32_Service -Filter "Name='condepnode'"

    if($wmiService) {
        write-host 'ConDepNode allready exist. Stopping and removing now...'
        if($wmiService.State -ne "Stopped") {
            write-host 'ConDepNode is not stopped. Stopping now...'
    	    $service = Get-Service condepnode -ErrorAction Stop
			$service.Stop()
			$service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped)
        }
        write-host 'Deleting ConDepNode now...'
        $deleteResult = $wmiService.Delete()
        if($deleteResult.ReturnValue -ne 0) {
            throw "Failed to delete ConDepNode service. Return code was $($deleteResult.ReturnValue)"
        }
        write-host 'ConDepNode deleted'
    }

    write-host 'Adding firewall rule for ConDepNode'
    netsh advfirewall firewall delete rule name='ConDepNode'
    netsh advfirewall firewall add rule name='ConDepNode' protocol=TCP localport=80 action=allow dir=IN
    write-host 'Creating new ConDepNode...'
    New-Service -Name ConDepNode -BinaryPathName "$remFile $nodeUrl" -StartupType Manual
    write-host 'ConDepNode created'
    write-host 'Starting ConDepNode...'
    $service = get-service condepnode
    $service.Start()
    write-host 'Waiting for node to start...'
	$service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
    write-host 'ConDepNode started'

    $conDepReturnValues = New-Object PSObject -Property @{           
        ConDepResult    = New-Object PSObject -Property @{
            Success = $true
        }                 
    }                     
   
    return $conDepReturnValues

}