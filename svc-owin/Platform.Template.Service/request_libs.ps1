Param (
   [Parameter(Mandatory=$true)][string] $Identity,
   [Parameter(Mandatory=$true)][string] $ServiceRootPath
)
$buildConf = "Debug"
$workingDir = (Get-Item -Path ".\" -Verbose).FullName
$toolAuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f "eventmanager","Realine2016")))
$getLibUrl = "http://master.s1.test.domination.win/Platform.Events.Web.new/service/GetServiceLibraries"
$cloud = "demo"
$nugetServerUrl = "http://54.187.86.186:10008/nuget/"
$nugetSources = "C:\Users\Administrator2\.nuget\packages\;C:\Users\Administrator\AppData\Local\NuGet\Cache;C:\Users\Administrator\AppData\Local\NuGet\v3-cache;C:\Windows\system32\config\systemprofile\AppData\Local\NuGet\Cache\;C:\Windows\system32\config\systemprofile\.nuget\packages\;https://api.nuget.org/v3/index.json;http://54.187.86.186:10008/nuget/"
$nuget = "C:\tools\nuget.exe"

Add-Type -AssemblyName System.IO.Compression.FileSystem

$mergedPackages = New-Object System.Collections.ArrayList

$packages = New-Object System.Collections.ArrayList
$response =  Invoke-RestMethod -Method Get -Uri ($getLibUrl+"/?serviceIdentity="+$Identity+"&cloud="+$cloud)  -Headers @{Authorization=("Basic {0}" -f $toolAuthInfo)} 
Foreach($lib in $response)
{
	$obj = 	 @{       
			 LibraryName  = $lib.LibraryName
			 Version = $lib.Version
	}
	$packages.Add($obj)
}


function Unzip
{
    param([string]$zipfile, [string]$outpath)
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}

function DownloadNugetPackages
{
    param($serviceDir, [string]$nugetServerUrl, [System.Collections.ArrayList]$packages)

    
    $binFolder = $serviceDir #Join-Path $serviceDir ("bin\"+$buildConf+"\")
    $dropFolder = Join-Path $serviceDir ("\extensions")
    $customSqlFolder = Join-Path $serviceDir "extensionSql"

    $tempFolder = Join-Path $dropFolder "\temp"
    $tempDependencyFolder = Join-Path $dropFolder "\temp\dependencies"
    if((Test-Path -Path $tempFolder )){
        Remove-Item -Recurse -Force -Path $tempFolder
    }
    New-Item -ItemType directory -Path $tempFolder
    New-Item -ItemType directory -Path $tempDependencyFolder

    $packageNames = New-Object System.Collections.ArrayList
    foreach($package in $packages)
	{
        $packageNames.Add($package.LibraryName.Trim())
    }

    foreach($package in $packages){
        $package_stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        
        $nugetExpression = ($nuget + " install "+$package.LibraryName.Trim() + ' -version "'+$package.Version.Trim()+'" -source  "'+$nugetSources+'" -OutputDirectory "'+$tempDependencyFolder+'" -verbosity detailed')
        Write-Host $nugetExpression

        Invoke-Expression ($nuget + " install "+$package.LibraryName.Trim() + ' -version "'+$package.Version.Trim()+'" -source  "'+$nugetSources+'" -OutputDirectory "'+$tempDependencyFolder+'" -verbosity detailed')
        #Find main package
        $mainPackageDir = Get-ChildItem -Path $tempDependencyFolder  ($package.LibraryName.Trim()+"*") | Select -First 1
        $libPath = Join-Path $mainPackageDir.Fullname "lib\*"

        Write-Host ("nuget dependency install time: "+$package_stopwatch.Elapsed)
        $package_stopwatch.Restart()
        
        $dlls = Get-ChildItem -Path $tempDependencyFolder -Directory
        foreach($dll in $dlls){
            
            #if($dll.Name.Contains("Platform.Utils.Json")){
            #  $kkkjf = 10
            #}
            $skip = $FALSE
            $mainPackageName = $package.LibraryName.Trim()
            #do not copy main packages libs
            foreach($packName in $packageNames){
                if($dll.Name.Contains($packName)){
                    $skip = $TRUE
                }
            }
            if($skip -eq $TRUE) { 
            
            continue 
            
            }
            $currentLib = Join-Path $dll.Fullname "lib"
            if(!(Test-Path $currentLib)) {continue}
            $netFolders = Get-ChildItem -Path $currentLib -Filter "net*" | sort-object name | Select -Last 1
            $newFoldersPath = $netFolders.FullName

            if(!$newFoldersPath){
                $newFoldersPath = $currentLib
            }

            $netDlls = Get-ChildItem -Path $newFoldersPath -Filter "*.dll"

            foreach($netDll in $netDlls){
                $source = $netDll.Fullname
                $destination = Join-Path $binFolder $netDll.Name
                if(!(Test-Path $destination)){
                    Write-Host ("Copy dependency: "+$netDll.Name)
                    Copy-Item $source $destination
                }
            }
            
        }

        Copy-Item ($libPath) $dropFolder -Filter *.dll
        #check sql
        $libSqlPath = Get-ChildItem -Path $destinationFolder sql | Select -First 1
        if($libSqlPath){
            if(!(Test-Path $customSqlFolder)){ New-Item $customSqlFolder -type directory }
            $sqlFiles = Get-ChildItem $libSqlPath.FullName *.sql
            foreach($sqlFile in $sqlFiles){
                $sqlFileNewName = ($package.LibraryName+"_"+$sqlFile.Name)
                $sqlFileNewLocation = Join-Path $customSqlFolder $sqlFileNewName
                Copy-Item $sqlFile.FullName $sqlFileNewLocation
            }
        }
        Write-Host ("package dependency download time: "+$package_stopwatch.Elapsed)
    }

    try { Remove-Item -Recurse -Force -Path $tempFolder  } 
    catch { "remove error" }
}

function MergeDependencies($libPath, $serviceDir){
    Write-Host ("Merging dependencies: "+$libPath)
    #Find nuspec
    $nuspecFile = Get-ChildItem -Path $libPath -Filter *.nuspec -Recurse | Select -First 1
    $servicePackagesFile = Get-ChildItem -Path $serviceDir.Fullname -Filter "packages.config" -Recurse | Select -First 1
    Write-Host ("nuspec file: "+$nuspecFile.Fullname)
    $packageXml = [xml](Get-Content $nuspecFile.FullName)
    $serviceXml = [xml](Get-Content $servicePackagesFile.FullName)
    $dependencyNode = $packageXml.package.metadata.dependencies
    foreach($packageDependency in $dependencyNode.ChildNodes){
        Write-Host $packageDependency
        $existingDependency = GetServiceDependency $packageDependency.id $serviceXml
        if($existingDependency -ne $null){
            Write-Host "package exists"
        }else {
            Write-Host "package not exists"
            #adding
            $packageNode = $serviceXml.CreateElement("package");
            $packageNode.SetAttribute("id", $packageDependency.id);
            $packageNode.SetAttribute("version", $packageDependency.version);
            $packageNode.SetAttribute("targetFramework", "net451");
            $serviceXml.packages.AppendChild($packageNode)
            #reference to project
            $mergedPackages.Add($packageDependency);
        }
    }
    $serviceXml.save($servicePackagesFile.FullName)
}

function GetServiceDependency($packageId, $servicePackagesXml){
    foreach($package in $servicePackagesXml.packages.ChildNodes){
        if($package.id -eq $packageId){
            return $package;
        }
    }
    return $null;
}

DownloadNugetPackages $ServiceRootPath $nugetServerUrl $packages