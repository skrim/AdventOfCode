namespace Skrim.AdventOfCode

open System

type Task20221224 () =

    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let width = input |> Seq.head |> Seq.length
            let height = input |> Seq.length
            let start = (1, 0)
            let goal = (width - 2, height - 1)

            let blizzards = // east, south, west, north
                input
                |> Seq.skip 1
                |> Seq.take (height - 2)
                |> Seq.fold(fun (maps:bool list list list) line ->
                    let lineResult =
                        line
                        |> Seq.skip 1
                        |> Seq.take (width - 2)
                        |> Seq.fold(fun lineOut tile ->
                            let (w, s, e, n) =
                                match tile with
                                | '.' -> (false, false, false, false)
                                | '>' -> (true, false, false, false)
                                | 'v' -> (false, true, false, false)
                                | '<' -> (false, false, true, false)
                                | '^' -> (false, false, false, true)
                                | _ -> raise <| new InvalidOperationException()

                            [ w :: lineOut[0]; s :: lineOut[1]; e :: lineOut[2]; n :: lineOut[3] ]
                        ) ((Array.create 4 []) |> Array.toList)

                    let merge n = (lineResult[n] |> List.rev) :: maps[n]
                    [ merge 0; merge 1; merge 2; merge 3 ]
                ) [ []; []; []; [] ]
                |> List.map List.rev

            let (walls, _) =
                {0 .. height * width - 1}
                |> Seq.fold (fun (map, row) pos ->
                    match pos % width, pos / width with
                    | p when p = start || p = goal -> (map, false :: row)
                    | x, _ when x = width - 1 -> ((true :: row) :: map, [])
                    | x, y when x = 0 || y = 0 || y = height - 1 -> (map, true :: row)
                    | _ -> (map, false :: row)
                ) ([], [])

            let getState round =
                let wrap index delta length =
                    match (index + delta) % length with
                    | np when np < 0 -> length + np
                    | np -> np

                let mergeBlizzard x y =
                    let bw, bh, d = width - 2, height - 2, round
                    blizzards[0].[y].[wrap x -d bw] || blizzards[1].[wrap y -d bh].[x] ||
                    blizzards[2].[y].[wrap x d bw] || blizzards[3].[wrap y d bh].[x]

                {0 .. height * width - 1}
                |> Seq.fold (fun (map:bool list list) pos ->
                    match pos % width, pos / width with
                    | x, y when x = 0 || y = 0 || x = width - 1 || y = height - 1 -> map
                    | x, y ->
                        let newRow = map[y] |> List.updateAt x (mergeBlizzard (x - 1) (y - 1))
                        map |> List.updateAt y newRow
                ) walls

            let moveList = [ (0, 0); (1, 0); (0, 1); (-1, 0); (0, -1) ]

            let routeLength targetList =
                (Set.singleton (targetList |> List.head), 0, targetList |> List.tail)
                |> Seq.unfold(fun (positions, round, targets) ->
                    let nextState = getState round

                    let nextPositions acc (px, py) =
                        let addIfValid moves (dx, dy) =
                            match nextState, (px + dx, py + dy) with
                            | _, (x, y) when x < 0 || y < 0 || x >= width || y >= height -> moves
                            | _, (x, y) when nextState[y].[x] -> moves
                            | _, (x, y) -> moves |> Set.add (x, y)

                        moveList |> List.fold addIfValid acc

                    let newPositions = positions |> Set.fold nextPositions Set.empty

                    match targets with
                    | target::[] when newPositions |> Set.contains target -> None
                    | target::tail when newPositions |> Set.contains target -> Some (round, (Set.singleton target, round + 1, tail))
                    | _ -> Some (round, (newPositions, round + 1, targets))
                )
                |> Seq.max
                |> (+) 1

            (routeLength [ start; goal ], routeLength [ start; goal; start; goal ])