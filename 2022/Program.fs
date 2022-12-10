namespace Skrim.AdventOfCode

open System
open System.Diagnostics
open System.IO
open System.Reflection

type Program () =
    static let getInstances types =
        types
        |> Array.map (fun t -> Activator.CreateInstance t)
        |> Array.map (fun t -> t :?> IAdventOfCodeTask)

    [<EntryPoint>]
    static let main args =
        let writeColor text column =
            Console.ForegroundColor <- [| ConsoleColor.White; ConsoleColor.Green; ConsoleColor.Green; ConsoleColor.Cyan |][column]
            printf "%s" text
            Console.ResetColor()

        let types =
            Assembly.GetCallingAssembly().GetTypes()
            |> Array.filter (fun t -> typedefof<IAdventOfCodeTask>.IsAssignableFrom t)
            |> Array.filter (fun t -> not t.IsInterface)
            |> Array.sortBy (fun t -> t.Name)

        printfn "Solving %d tasks..." (Array.length types)

        let answers =
            getInstances types
            |> Array.map (fun x ->
                let taskType = x.GetType()
                let taskName = taskType.Name
                let input = System.IO.File.ReadAllLines(Path.Join("data", sprintf "%s.txt" taskName))
                let sw = Stopwatch.StartNew()
                let result =
                    if typeof<IAdventOfCodeTask<int64>>.IsAssignableFrom taskType then
                        let task = x :?> IAdventOfCodeTask<int64>
                        let answer = task.Solve(input)
                        [| taskName; answer |> fst |> string; answer |> snd |> string |]
                    elif typeof<IAdventOfCodeTask<int64, string>>.IsAssignableFrom taskType then
                        let task = x :?> IAdventOfCodeTask<int64, string>
                        let answer = task.Solve(input)
                        [| taskName; answer |> fst |> string; answer |> snd |]
                    elif typeof<IAdventOfCodeTask<string>>.IsAssignableFrom taskType then
                        let task = x :?> IAdventOfCodeTask<string>
                        let answer = task.Solve(input)
                        [| taskName; answer |> fst; answer |> snd |]
                    else
                        raise <| new InvalidOperationException()
                sw.Stop()
                [| (sw.ElapsedMilliseconds |> string) + " ms"|] |> Array.append result
            )
            |> Array.map (fun a ->
                a |> Array.map (fun v -> v.Split("\n"))
            )

        let columnSizes =
            [| 0..3 |]
            |> Array.map(fun index ->
                answers
                |> Array.map(fun row ->
                    row[index])
                    |> Array.map (fun f ->
                        f |> Array.map String.length |> Array.max
                    )
                    |> Array.max
                )

        for row in answers do
            let lines = row |> Array.map Array.length |> Array.max
            for line in { 1..lines } do
                for column in { 0..3 } do
                    let size = columnSizes[column]
                    let value = row[column]
                    if Array.length value < line then
                        for _ in { 1..size } do printf " "
                    else
                        let p = value[line - 1]
                        for _ in { 1..size - String.length p } do printf " "
                        writeColor (sprintf "%s" p) column
                    printf "  |  "
                printfn ""

        0