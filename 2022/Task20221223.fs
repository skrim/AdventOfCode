namespace Skrim.AdventOfCode

type Task20221223 () =

    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let initialState =
                input
                |> Seq.fold (fun (y, state) line ->
                    let lineResult =
                        line
                        |> Seq.fold (fun (x, state) tile ->
                            match tile with
                            | '#' -> (x + 1, state |> Set.add (x, y))
                            | _ -> (x + 1, state)
                        ) (0, state)
                    (y + 1, snd lineResult)
                ) (0, Set.empty)
                |> snd

            let allLookups = [
                    [ (0, -1); (-1, -1); (1, -1) ];
                    [ (0, 1); (-1, 1); (1, 1) ];
                    [ (-1, 0); (-1, -1); (-1, 1) ];
                    [ (1, 0); (1, -1); (1, 1) ]
                ]

            let allNeighbors = allLookups |> List.concat |> List.distinct

            let getBounds state = (
                    state |> Set.map fst |> Set.minElement, state |> Set.map snd |> Set.minElement,
                    state |> Set.map fst |> Set.maxElement, state |> Set.map snd |> Set.maxElement
                ) //minX, minY, maxX, maxY

            let move step state =
                let shift = allLookups |> List.take (step % 4)
                let currentLookups = (allLookups |> List.skip (step % 4)) @ shift

                let hasNeighbor (cx, cy) neighbors = neighbors |> List.tryFind(fun (dx, dy) -> state |> Set.contains (cx + dx, cy + dy)) <> None

                let rec findDirection (x, y) = function
                    | l::t when hasNeighbor (x, y) l -> findDirection (x, y) t
                    | ((mx, my) :: _) :: _ -> ((x, y), (x + mx, y + my))
                    | _ -> ((x, y), (x, y))

                let nextPosition pos =
                    match hasNeighbor pos allNeighbors with
                    | false -> (pos, pos)
                    | true -> findDirection pos currentLookups

                let nextPositions = state |> Set.map nextPosition

                let conflicts =
                    nextPositions
                    |> Set.toList
                    |> List.countBy snd
                    |> List.filter(fun (_, c) -> c >= 2)
                    |> List.map fst
                    |> Set.ofList

                let nextState =
                    nextPositions
                    |> Set.map (fun (fromPosition, toPosition) ->
                        match conflicts |> Set.contains toPosition with
                        | true -> fromPosition
                        | false -> toPosition
                    )

                (state <> nextState, nextState)

            let finalState = { 0..9 } |> Seq.fold(fun state round -> move round state |> snd) initialState
            let minX, minY, maxX, maxY = getBounds finalState
            let part1 = (maxX - minX + 1) * (maxY - minY + 1) - (finalState |> Set.count)

            let part2 =
                (initialState, 0)
                |> Seq.unfold(fun (state, round) ->
                    match move round state with
                    | (false, _) -> None
                    | (true, s) -> Some (round, (s, round + 1))
                )
                |> Seq.max
                |> (+) 2

            (part1, part2)