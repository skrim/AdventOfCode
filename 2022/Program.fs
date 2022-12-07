namespace Skrim.AdventOfCode

open System
open System.IO
open System.Reflection

type Program () =
    static let getInstances types =
        types
        |> Array.map (fun t -> Activator.CreateInstance t)
        |> Array.map (fun t -> t :?> IAdventOfCodeTask)

    [<EntryPoint>]
    static let main args =
        let writeColor text color =
            Console.ForegroundColor <- color
            printfn "%s" text
            Console.ResetColor()

        let types =
            Assembly.GetCallingAssembly().GetTypes()
            |> Array.filter (fun t -> typedefof<IAdventOfCodeTask>.IsAssignableFrom t)
            |> Array.filter (fun t -> not t.IsInterface)
            |> Array.sortBy (fun t -> t.Name)

        printfn "Solving %d tasks..." (Array.length types)

        getInstances types
        |> Array.mapi (fun i x ->
            let taskType = x.GetType()
            let taskName = taskType.Name
            let sw = Diagnostics.Stopwatch.StartNew()
            let input = System.IO.File.ReadAllLines(Path.Join("data", sprintf "%s.txt" taskName))
            if typeof<IAdventOfCodeTask<int64>>.IsAssignableFrom taskType then
                let int64task = x :?> IAdventOfCodeTask<int64>
                let answer = int64task.Solve(input)
                writeColor (sprintf "%s: %10d %10d" (taskName) (answer |> fst) (answer |> snd)) ConsoleColor.Gray
            elif typeof<IAdventOfCodeTask<string>>.IsAssignableFrom taskType then
                let stringtask = x :?> IAdventOfCodeTask<string>
                let answer = stringtask.Solve(input)
                writeColor (sprintf "%s: %10s %10s" (taskName) (answer |> fst) (answer |> snd)) ConsoleColor.Gray

            sw.Stop();
            0)
        |> ignore

        0