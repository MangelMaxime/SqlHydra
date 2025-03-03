﻿module SqlHydra.Fsproj
open System.IO
open System.Collections.Generic
open Microsoft.Build.Construction
open Domain

/// Adds the generated .fs file to the fsproj as Visible=False.
let addFileToProject (cfg: Config) = 
    Directory.EnumerateFiles(".", "*.fsproj") |> Seq.tryHead
    |> Option.iter (fun fsprojPath ->
        let root = ProjectRootElement.Open(fsprojPath)

        let fileAlreadyAdded = 
            root.ItemGroups 
            |> Seq.collect (fun grp -> grp.Items)
            |> Seq.exists (fun item -> 
                item.Include = cfg.OutputFile || 
                item.Include = cfg.OutputFile.Replace(@"\", "/") || // Handle "Folder/File.fs"
                item.Include = cfg.OutputFile.Replace("/", @"\")    // Handle "Folder\Files.fs"
            )

        root.ItemGroups
        |> Seq.filter (fun g -> g.Items |> Seq.exists (fun item -> item.ItemType = "Compile"))
        |> Seq.tryHead
        |> Option.iter (fun fstCompileGrp ->
            if not fileAlreadyAdded then
                printfn $"Adding '{cfg.OutputFile}' to .fsproj."
                fstCompileGrp.AddItem("Compile", cfg.OutputFile, [ KeyValuePair("Visible", "False") ]) |> ignore
                root.Save()
        )
    )
