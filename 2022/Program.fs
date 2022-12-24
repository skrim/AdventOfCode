namespace Skrim.AdventOfCode

open System
open System.Diagnostics
open System.IO
open System.Reflection

type Program () =
    static let getInstances types =
        types
        |> List.map (fun t -> Activator.CreateInstance t)
        |> List.map (fun t -> t :?> IAdventOfCodeTask)

    [<EntryPoint>]
    static let main args =
        let writeColor text column =
            Console.ForegroundColor <- [| ConsoleColor.White; ConsoleColor.Green; ConsoleColor.Green; ConsoleColor.Cyan |][column]
            printf "%s" text
            Console.ResetColor()

        let argFilter =
            match args |> Array.length with
            | 1 -> (fun (t:Type) -> t.Name.Contains(args[0]))
            | _ -> fun _ -> true

        let types =
            Assembly.GetCallingAssembly().GetTypes()
            |> Array.toList
            |> List.filter (fun t -> typedefof<IAdventOfCodeTask>.IsAssignableFrom t)
            |> List.filter (fun t -> not t.IsInterface)
            |> List.filter argFilter
            |> List.sortBy (fun t -> t.Name)

        printfn "Solving %d tasks..." (List.length types)

        let results =
            getInstances types
            |> List.map (fun x ->
                let taskType = x.GetType()
                let taskName = taskType.Name
                let fileName = Path.Join("data", sprintf "%s.txt" taskName)
                if not (File.Exists fileName) then File.WriteAllText(fileName, "")
                let input = System.IO.File.ReadAllLines(fileName)
                let sw = Stopwatch.StartNew()
                let result =
                    if typeof<IAdventOfCodeTask<int64>>.IsAssignableFrom taskType then
                        let task = x :?> IAdventOfCodeTask<int64>
                        let answer = task.Solve(input)
                        [ answer |> fst |> string; answer |> snd |> string ]
                    elif typeof<IAdventOfCodeTask<int64, string>>.IsAssignableFrom taskType then
                        let task = x :?> IAdventOfCodeTask<int64, string>
                        let answer = task.Solve(input)
                        [ answer |> fst |> string; answer |> snd ]
                    elif typeof<IAdventOfCodeTask<string>>.IsAssignableFrom taskType then
                        let task = x :?> IAdventOfCodeTask<string>
                        let answer = task.Solve(input)
                        [ answer |> fst; answer |> snd ]
                    else
                        raise <| new InvalidOperationException(taskType.Name + " has unknown type parameters in its interface")
                sw.Stop()
                (taskName :: result @ [ (sw.ElapsedMilliseconds |> string) + " ms" ], sw.ElapsedMilliseconds)
            )

        let totalTime = results |> List.map snd |> List.sum

        let answers =
            [([ "Total"; ""; ""; (totalTime |> string) + " ms" ], 0L)]
            |> List.append results
            |> List.map (fun a ->
                a |> fst |> List.map (fun v -> v.Split("\n") |> Array.toList)
            )

        let columnSizes =
            [ 0..3 ]
            |> List.map(fun index ->
                answers
                |> List.map(fun row ->
                    row[index])
                    |> List.map (fun f ->
                        f |> List.map String.length |> List.max
                    )
                    |> List.max
                )

        for row in answers do
            let lines = row |> List.map List.length |> List.max
            for line in { 1..lines } do
                for column in { 0..3 } do
                    let size = columnSizes[column]
                    let value = row[column]
                    if List.length value < line then
                        for _ in { 1..size } do printf " "
                    else
                        let p = value[line - 1]
                        for _ in { 1..size - String.length p } do printf " "
                        writeColor (sprintf "%s" p) column
                    printf " | "
                printfn ""

        0