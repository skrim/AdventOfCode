namespace Skrim.AdventOfCode

type Task20221208 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =

            let width = (Seq.head input).Length
            let height = Seq.length input

            let grid =
                input
                |> Seq.toArray
                |> Array.map(fun f -> f |> Seq.map (fun v -> (v |> int32) - 48) |> Seq.toArray)

            let forAllDirections x y functionCall aggregator =
                let partials = [
                    fun x y -> seq { for i in x - 1 .. -1 ..    0 do yield grid[y].[i] };
                    fun x y -> seq { for i in x + 1 ..  width - 1 do yield grid[y].[i] };
                    fun x y -> seq { for i in y - 1 .. -1 ..    0 do yield grid[i].[x] };
                    fun x y -> seq { for i in y + 1 .. height - 1 do yield grid[i].[x] }
                ]
                let callback dirPart = dirPart x y |> functionCall grid[y].[x]

                List.tail partials
                |> List.fold(fun state value -> aggregator state (value |> callback) ) (List.head partials |> callback)

            let isVisible tree others = others |> Seq.filter(fun h -> h >= tree) |> Seq.isEmpty

            let directionScore tree others =
                others |>
                Seq.fold(fun state current ->
                    match state with
                    | (_, true) -> state
                    | (score, false) when current >= tree -> (score + 1, true)
                    | (score, false) -> (score + 1, false)
                ) (0, false)
                |> fst

            let visibilityAndScore =
                [| 0 .. width - 1 |]
                |> Array.map(fun x ->
                    [| 0 .. height - 1 |]
                    |> Array.map(fun y ->
                        let visible = forAllDirections x y isVisible (||)
                        let score = if visible then forAllDirections x y directionScore (*) else 0

                        (visible, score)
                    )
                )
                |> Array.fold Array.append Array.empty

            let part1 = visibilityAndScore |> Array.filter fst |> Array.length
            let part2 = visibilityAndScore |> Array.map snd |> Array.max

            (part1, part2)