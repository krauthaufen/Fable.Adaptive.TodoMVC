#r "paket: groupref Build //"
#load ".fake/build.fsx/intellisense.fsx"

open System
open System.IO
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Tools
open System.Text.RegularExpressions

do Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

let isWindows =
    Environment.OSVersion.Platform <> PlatformID.Unix && Environment.OSVersion.Platform <> PlatformID.MacOSX


let del (dir : string) =
    if Directory.Exists dir then
        Trace.tracefn "deleting %s" dir
        Directory.delete dir
    elif File.Exists dir then
        Trace.tracefn "deleting %s" dir
        File.delete dir
        

Target.create "Clean" (fun _ ->
    del "bin/Debug"
    del "bin/Release"
    del "bin/Fable"
    del ".fable"

    let obj = !!"src/**/obj/"
    for o in obj do
        let isProject = Directory.GetFiles(Path.GetDirectoryName o, "*.fsproj").Length > 0
        if isProject then del o

    let pkgs = !!"bin/*.nupkg" |> Seq.toList
    if not (List.isEmpty pkgs) then
        Trace.tracefn "deleting packages: %s" (pkgs |> Seq.map Path.GetFileNameWithoutExtension |> String.concat ", ")
        File.deleteAll pkgs


    let dirs = Directory.EnumerateDirectories("src", "obj", SearchOption.AllDirectories) |> Seq.toList

    if not (List.isEmpty dirs) then 
        for d in dirs do    
            let parent = Path.GetDirectoryName d
            let proj = Directory.GetFiles(parent, "*.fsproj") |> Seq.isEmpty |> not
            if proj then
                Trace.tracefn "deleting %s" d
                Directory.delete d
)

Target.create "DotNetCompile" (fun _ ->
    let options (o : DotNet.BuildOptions) =
        { o with 
            Configuration = DotNet.BuildConfiguration.Release
        }
    DotNet.build options "Fable.Adaptive.TodoMVC.sln"
)

Target.create "NpmInstall" (fun _ ->
    let modules = "node_modules" |> Path.GetFullPath

    if not (Directory.Exists modules) then
        Trace.trace "running `npm install`"

        let npm =
            if isWindows then CreateProcess.fromRawCommand "cmd" ["/C"; "npm"; "install"; "--dev"]
            else CreateProcess.fromRawCommand "npm" ["install"]

        use s = new MemoryStream()
        npm
        |> CreateProcess.withWorkingDirectory Environment.CurrentDirectory
        |> CreateProcess.withStandardError (StreamSpecification.UseStream(true, s))
        |> CreateProcess.withStandardOutput  (StreamSpecification.UseStream(true, s))
        |> Proc.run
        |> ignore

)

Target.create "Watch" (fun _ ->
    let wpds = "node_modules/webpack-dev-server/bin/webpack-dev-server.js" |> Path.GetFullPath
    CreateProcess.fromRawCommand "node" [wpds]
    |> CreateProcess.withWorkingDirectory Environment.CurrentDirectory
    |> CreateProcess.withStandardError StreamSpecification.Inherit
    |> CreateProcess.withStandardOutput StreamSpecification.Inherit
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

)
Target.create "Debug" (fun _ ->
    let wpds = "node_modules/webpack-cli/bin/cli.js" |> Path.GetFullPath
    CreateProcess.fromRawCommand "node" [wpds; "-p"; "-g"]
    |> CreateProcess.withWorkingDirectory Environment.CurrentDirectory
    |> CreateProcess.withStandardError StreamSpecification.Inherit
    |> CreateProcess.withStandardOutput StreamSpecification.Inherit
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

)

Target.create "Release" (fun _ ->
    let wpds = "node_modules/webpack-cli/bin/cli.js" |> Path.GetFullPath
    CreateProcess.fromRawCommand "node" [wpds; "-p"]
    |> CreateProcess.withWorkingDirectory Environment.CurrentDirectory
    |> CreateProcess.withStandardError StreamSpecification.Inherit
    |> CreateProcess.withStandardOutput StreamSpecification.Inherit
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

)

Target.create "RunDebug" (fun _ ->
    let http = "node_modules/local-web-server/bin/cli.js" |> Path.GetFullPath
    let outDir = "bin/Fable/Debug" |> Path.GetFullPath
    CreateProcess.fromRawCommand "node" [http; "--port"; "8080"; "--directory"; outDir]
    |> CreateProcess.withStandardError StreamSpecification.Inherit
    |> CreateProcess.withStandardOutput StreamSpecification.Inherit
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore
)

Target.create "RunRelease" (fun _ ->
    let http = "node_modules/local-web-server/bin/cli.js" |> Path.GetFullPath
    let outDir = "bin/Fable/Release" |> Path.GetFullPath
    CreateProcess.fromRawCommand "node" [http; "--port"; "8080"; "--directory"; outDir]
    |> CreateProcess.withStandardError StreamSpecification.Inherit
    |> CreateProcess.withStandardOutput StreamSpecification.Inherit
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore
)


Target.create "Default" ignore

"Clean" ==>
    "NpmInstall" ==> 
    "DotNetCompile" ==>
    "Watch"

"Clean" ==>
    "NpmInstall" ==> 
    "DotNetCompile" ==>
    "Debug"

"Clean" ==>
    "NpmInstall" ==> 
    "DotNetCompile" ==>
    "Release"

"Debug" ==> "RunDebug"
"Release" ==> "RunRelease"

"Debug" ==> 
    "Default"


Target.runOrDefault "Default"


