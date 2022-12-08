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
                let leftPart x y = grid[y].[.. x - 1] |> Array.rev
                let rightPart x y = grid[y].[x + 1 ..]
                let topPart x y = grid[.. y - 1] |> Array.map(fun col -> col[x]) |> Array.rev
                let bottomPart x y = grid[y + 1 ..] |> Array.map(fun col -> col[x])

                let callback dirPart = dirPart x y |> functionCall grid[y].[x]
                ( (callback leftPart, callback rightPart) ||> aggregator, (callback topPart, callback bottomPart) ||> aggregator ) ||> aggregator

            let isVisible tree others = others |> Array.filter(fun h -> h >= tree) |> Array.isEmpty

            let directionScore tree others =
                others |>
                Array.fold(fun state current ->
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
                        let score = forAllDirections x y directionScore (*)

                        (visible, score)
                    )
                )
                |> Array.fold Array.append Array.empty

            let part1 = visibilityAndScore |> Array.filter fst |> Array.length
            let part2 = visibilityAndScore |> Array.map snd |> Array.max

            (part1, part2)