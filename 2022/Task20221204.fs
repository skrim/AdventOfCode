namespace Skrim.AdventOfCode

type Task20221204 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let sections =
                input
                |> Array.ofSeq
                |> Array.map(fun f ->
                    f.Split(',')
                    |> Array.map(fun v -> v.Split('-') |> Array.map int32)
                    |> Array.map(fun v -> (Seq.min v, Seq.max v))
                )
                |> Array.map (fun f -> (f[0], f[1]))

            let filter1 (a, b) =
                match a, b with
                | (a1, a2), (b1, b2) when a1 >= b1 && a2 <= b2 -> true
                | (a1, a2), (b1, b2) when a1 <= b1 && a2 >= b2 -> true
                | _ -> false

            let filter2 (a, b) =
                match a, b with
                | (a1, a2), (b1, b2) when a2 < b1 || a1 > b2 -> false
                | _ -> true

            let matcher filter = sections |> Array.filter filter |> Array.length

            (matcher filter1, matcher filter2)