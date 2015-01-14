   Param
( 
    [string]$SrcPath = $env:TF_BUILD_SOURCESDIRECTORY,
    [string]$SrcBinPath = $env:TF_BUILD_BINARIESDIRECTORY,
    [string]$NugetPath = $PSScriptRoot,
    [string]$PackageVersion = $env:TF_BUILD_BUILDNUMBER,
    [string]$NugetServer = "http://isd-aps2:6566/",
	[string]$packagesExportLocation = $SrcBinPath + "\package",
    [string]$NugetUserAPIKey = "d756e5db-de18-4be9-9c32-6e7fcc6688cd"
     )

    $AllNugetPackageFiles = Get-ChildItem $packagesExportLocation\*.nupkg
   Write-Host "Packages " $AllNugetPackageFiles
       foreach ($file in $AllNugetPackageFiles)
    { 
    Write-Host "Publishing package"  $file
        #Create the .nupkg from the nuspec file
        $ps = new-object System.Diagnostics.Process
        $ps.StartInfo.Filename = "$NugetPath\Nuget.exe"
        $ps.StartInfo.Arguments = "push `"$file`" -s $NugetServer $NugetUserAPIKey"
        $ps.StartInfo.RedirectStandardOutput = $True
        $ps.StartInfo.RedirectStandardError = $True
        $ps.StartInfo.UseShellExecute = $false
        $ps.start()
        if(!$ps.WaitForExit(30000)) 
        {
            $ps.Kill()
        }
        [string] $Out = $ps.StandardOutput.ReadToEnd();
        [string] $ErrOut = $ps.StandardError.ReadToEnd();
        Write-Host "Nuget push Output of commandline " + $ps.StartInfo.Filename + " " + $ps.StartInfo.Arguments
        Write-Host $Out
        if ($ErrOut -ne "") 
        {
            Write-Error "Nuget push Errors"
            Write-Error $ErrOut
        }
 
    }