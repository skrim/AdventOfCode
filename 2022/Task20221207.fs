namespace Skrim.AdventOfCode

open System
open System.Text.RegularExpressions

type Task20221207 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =

            let readLine (fileMap, dirs:Set<string>, path:String) value =
                let (|Long|_|) str = Some(str |> int64)

                let (|ParseRegex|_|) regex str =
                    let m = Regex(regex).Match(str)
                    if m.Success then
                        Some (List.tail [ for x in m.Groups -> x.Value ])
                    else
                        None

                match value with
                | ParseRegex "^\$ cd /$" [] -> (fileMap, dirs, "/")
                | ParseRegex "^\$ cd \.\.$" [] when path.Split('/') |> Array.length = 2 -> (fileMap, dirs, "/")
                | ParseRegex "^\$ cd \.\.$" [] ->
                    let parentDir = path.Split('/') |> Array.rev |> Array.tail |> Array.tail |> Array.rev
                    (fileMap, dirs, String.Join('/', parentDir) + "/")
                | ParseRegex "^\$ cd ([a-z]+)$" [dir] ->
                    let newPath = path + dir + "/"
                    (fileMap, dirs.Add(newPath), newPath)
                | ParseRegex "^(\d+) ([a-z\.]+)$" [Long size; filename] ->
                    (fileMap |> Map.add (path + filename) size, dirs, path)
                | _ -> (fileMap, dirs, path)

            let (f, d, _) =
                input
                |> Seq.fold readLine (Map.empty, Set.empty.Add("/"), "")

            let files = f |> Map.toList

            let directories =
                d
                |> Set.toList
                |> List.map(fun dir ->
                    let size =
                        files
                        |> List.filter(fun (key, _) -> key.StartsWith(dir))
                        |> List.map snd
                        |> List.sum
                    (dir, size)
                )

            let part1 =
                directories
                |> List.map snd
                |> List.filter(fun f -> f <= 100000L)
                |> List.sum

            let totalUsed = files |> List.map snd |> List.sum

            let totalSpace = 70000000L
            let spaceNeeded = 30000000L
            let needFreeing = totalUsed - (totalSpace - spaceNeeded)

            let part2 =
                directories
                |> List.filter(fun (_, size) -> size >= needFreeing)
                |> List.sortBy(fun (_, size) -> size)
                |> List.head
                |> snd

            (part1, part2)