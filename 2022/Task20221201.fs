namespace Skrim.AdventOfCode

type Task20221201 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let groups =
                ([0L], input)
                ||> Seq.fold (fun state value ->
                    match value with
                    | "" -> 0L :: state
                    | _ -> List.head state + (int64 value) :: List.tail state
                )
                |> List.sortDescending

            (List.head groups, List.take 3 groups |> List.sum)