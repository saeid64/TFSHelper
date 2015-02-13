   Param
( 
    [string]$SrcPath = $env:TF_BUILD_SOURCESDIRECTORY,
    [string]$SrcBinPath = $env:TF_BUILD_BINARIESDIRECTORY,
    [string]$NugetPath = $PSScriptRoot,
    [string]$PackageVersion = $env:TF_BUILD_BUILDNUMBER,
	[string]$packagesExportLocation = $SrcBinPath + "\package"
     )
    Write-Host "Executing Publish-NugetPackage in path $SrcPath, PackageVersion is $PackageVersion"
    Write-Host "Transformed PackageVersion is $PackageVersion "
  
     $AllNugetFolders = gci -path $SrcPath -Recurse -Filter '.nuget'
	 $AllNuspecFiles = @()
	 foreach($f in $AllNugetFolders)
	 {
	 $files = gci $f.FullName -Filter '*.nuspec'
	 foreach ($f1 in $files){	
 $AllNuspecFiles += $f1
 }
	 }

    foreach ($file in $AllNuspecFiles)
    {
	$newFile = $file.CopyTo("$SrcBinPath\$file")
	$newFilePath = $newFile.FullName
	if(!(test-Path -Path $packagesExportLocation))
		{md -Path $packagesExportLocation}
    #$newPath = "$SrcBinPath$file.Name"
	#Write-Host "The new file path is: $newFile"
	Write-Host "The new file path is: $newFilePath"
	Write-Host "The package location is: $packagesExportLocation"
        #Create the .nupkg from the nuspec file
        $ps = new-object System.Diagnostics.Process
        $ps.StartInfo.Filename = "$NugetPath\Nuget.exe"
        $ps.StartInfo.Arguments = "pack `"$newFilePath`" -OutputDirectory ""$packagesExportLocation"""
        $ps.StartInfo.RedirectStandardOutput = $True
        $ps.StartInfo.RedirectStandardError = $True
        $ps.StartInfo.UseShellExecute = $false
        $ps.start()
        if(!$ps.WaitForExit(30000)) 
        {
            $ps.Kill()
        }
        [string] $Out = $ps.StandardOutput.ReadToEnd()
        [string] $ErrOut = $ps.StandardError.ReadToEnd()
        Write-Host "Nuget pack Output of commandline " + $ps.StartInfo.Filename + " " + $ps.StartInfo.Arguments
        Write-Host "Out is:" $Out
        if ($ErrOut -ne "") 
        {
            Write-Error "Nuget pack Errors"
            Write-Error $ErrOut
        }
        #Restore original file
        #Move-Item $backFile $file -Force
		remove-item $newFilePath -force
		Write-Host $newFilePath " Deleted"
    }