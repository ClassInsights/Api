Import-Module IISAdministration


if (git status --porcelain) {

    git pull
    dotnet restore
	
	# run publish
	$result = dotnet publish -c Release *>&1

	# Evaluate success/failure
	if($LASTEXITCODE -eq 0)
	{		
		# stop api and wait until it is stopped
		Stop-WebSite 'Api'
		$currentRetry = 0;
		$success = $false;
		do{
			$status = Get-WebsiteState -Name "Api"
			if ($status.Value -eq "Stopped") {
					Write-Host "API STOPPED!"
					Write-Host $status.Value
					$success = $true;
					Copy-Item -Path bin/Release/net7.0/publish/* -Force -Destination C:/inetpub/wwwroot
					Start-WebSite 'Api'
				}
				Start-Sleep -s 10
				$currentRetry = $currentRetry + 1;
			}
		while (!$success -and $currentRetry -le 4)		
	}    
}