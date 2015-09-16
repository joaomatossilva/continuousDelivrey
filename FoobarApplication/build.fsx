// include Fake lib
#r @".tools\FAKE\tools\FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open System.IO

RestorePackages()

// Directories
let buildDir  = @".\.build\"
let testDir   = @".\.test\"
let deployDir = @".\.deploy\"
let abosluteDeployDir = Path.GetFullPath(deployDir)

// tools
//let nunitVersion = GetPackageVersion packagesDir "NUnit.Runners"
//let nunitPath = @".tools\NUnit.Runners\"
let fxCopRoot = @".\Tools\FxCop\FxCopCmd.exe"
    
let buildMode = getBuildParamOrDefault "buildMode" "Release"

// version info
let buildNumber =
  match buildServer with
  | TeamCity -> buildVersion
  | _ -> "0"

let version = "1.1." + buildNumber

// Targets
Target "Clean" (fun _ -> 
    CleanDirs [buildDir; testDir; deployDir;]
)

Target "WriteAssemblyInfo" (fun _ ->
    CreateCSharpAssemblyInfo "./AssemblyInfo_Shared.cs"
        [Attribute.Version version
         Attribute.FileVersion version]
)

Target "CompileApp" (fun _ ->    
    !! @"**\*.csproj" 
//      |> MSBuildReleaseExt null [ ("RunOctoPack", "true"); ("OctoPackEnforceAddingFiles","true"); ("OctoPackPackageVersion", version); ("OctoPackPublishPackageToFileShare", abosluteDeployDir) ] "Build" 
      |> MSBuildReleaseExt null [ ("RunOctoPack", "true"); ("OctoPackEnforceAddingFiles","true"); ("OctoPackPackageVersion", version) ] "Build" 
      |> Log "AppBuild-Output: "
)


//TODO: Build project on splitted folders and zip them acordanly.
Target "Zip" (fun _ ->
    !! (buildDir + "\_PublishedWebsites\**\*.*")
        -- "*.zip" 
        |> Zip buildDir (deployDir + "CityOfLove.Web." + version + ".zip")
)

Target "Default" (fun _ ->
    trace "Built!"
)

Target "Release" (fun _ ->
    trace "Release Target! You've nailed it"
)


// Dependencies
"Clean"
  //==> "WriteAssemblyInfo"
  ==> "CompileApp" 
  //==> "CompileTest"
  //==> "FxCop"
  //==> "NUnitTest"  
  ==> "Default"
  ==> "Zip"
  ==> "Release"
 
// start build
RunTargetOrDefault "Default"