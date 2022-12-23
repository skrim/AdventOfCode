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

            let allNeighbors = allLookups |> List.concat |> Set.ofList

            let getBounds state = (
                    state |> Set.map fst |> Set.minElement, state |> Set.map snd |> Set.minElement,
                    state |> Set.map fst |> Set.maxElement, state |> Set.map snd |> Set.maxElement
                ) //minX, minY, maxX, maxY

            let move step state =
                let shift = allLookups |> List.take (step % 4)
                let currentLookups = (allLookups |> List.skip (step % 4)) @ shift

                let hasNeighbor (cx, cy) neighbors = neighbors |> Seq.tryFind(fun (dx, dy) -> state |> Set.contains (cx + dx, cy + dy)) <> None

                let rec nextPosition (x, y) lookups =
                    match hasNeighbor (x, y) allNeighbors, lookups with
                    | false, _ -> ((x, y), (x, y))
                    | true, l::_ when not(hasNeighbor (x, y) l) ->
                        let (mx, my) = l |> List.head
                        ((x, y), (x + mx, y + my))
                    | true, _::t -> nextPosition (x, y) t
                    | _, _ -> ((x, y), (x, y))

                let getConflictingTargets (positions:(int*int) list) =
                    positions |> List.countBy id |> List.filter(fun (_, c) -> c >= 2) |> List.map fst |> Set.ofList

                let nextPositions =
                    state |> Set.toList |> List.map (fun p -> nextPosition p currentLookups)

                let conflicts = getConflictingTargets (nextPositions |> List.map snd)

                let nextState =
                    nextPositions
                    |> List.map (fun (fromPosition, toPosition) ->
                        match conflicts |> Set.contains toPosition with
                        | true -> fromPosition
                        | false -> toPosition
                    )
                    |> Set.ofList

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
                |> Seq.rev
                |> Seq.head
                |> (+) 2

            (part1, part2)