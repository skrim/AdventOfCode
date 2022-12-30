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
                            | '#' -> (x + 1, (x, y) :: state)
                            | _ -> (x + 1, state)
                        ) (0, state)
                    (y + 1, snd lineResult)
                ) (0, List.empty)
                |> snd

            let directionLookups = [
                ( 0, -1, 32uy + 64uy + 128uy );
                ( 0, 1, 2uy + 4uy + 8uy);
                ( -1, 0, 8uy + 16uy + 32uy);
                ( 1, 0, 128uy + 1uy + 2uy)
            ]

            let getBounds state =
                let xSet, ySet = state |> List.map fst, state |> List.map snd
                (
                    xSet |> List.min, ySet |> List.min,
                    xSet |> List.max, ySet |> List.max
                ) //minX, minY, maxX, maxY

            let move step (state:(int*int) list) =
                let minX, minY, maxX, maxY = getBounds state
                let w = maxX - minX + 3
                let h = maxY - minY + 3
                let toIndex (x, y) = (x + 1 - minX) + (y + 1 - minY) * w
                let lookup = state |> List.fold(fun state pos -> state |> Array.updateAt (toIndex pos) 1uy) (Array.create (w * h) 0uy)

                let shift = directionLookups |> List.take (step % 4)
                let currentLookups = (directionLookups |> List.skip (step % 4)) @ shift

                let findNeighbors (cx, cy) =
                    (lookup[toIndex (cx + 1, cy    )] <<< 0) |||
                    (lookup[toIndex (cx + 1, cy + 1)] <<< 1) |||
                    (lookup[toIndex (cx    , cy + 1)] <<< 2) |||
                    (lookup[toIndex (cx - 1, cy + 1)] <<< 3) |||
                    (lookup[toIndex (cx - 1, cy    )] <<< 4) |||
                    (lookup[toIndex (cx - 1, cy - 1)] <<< 5) |||
                    (lookup[toIndex (cx    , cy - 1)] <<< 6) |||
                    (lookup[toIndex (cx + 1, cy - 1)] <<< 7)

                let rec findDirection (x, y) neighbors = function
                    | (dx, dy, mask) :: _ when neighbors &&& mask = 0uy -> ((x, y), (x + dx, y + dy))
                    | _::t -> findDirection (x, y) neighbors t
                    | _ -> ((x, y), (x, y))

                let nextPosition pos =
                    match findNeighbors pos with
                    | 0uy -> (pos, pos)
                    | v -> findDirection pos v currentLookups

                let nextPositions = state |> List.map nextPosition

                let conflicts =
                    nextPositions
                    |> List.countBy snd
                    |> List.filter(fun (_, c) -> c >= 2)
                    |> List.map fst
                    |> Set.ofList

                let (nextState, changed) =
                    nextPositions
                    |> List.fold (fun (ns, c) (fromPosition, toPosition) ->
                        match conflicts |> Set.contains toPosition with
                        | true -> (fromPosition :: ns, c || fromPosition <> toPosition)
                        | false -> (toPosition :: ns, c || fromPosition <> toPosition)
                    ) (List.empty, false)

                (nextState, changed)

            let part1 =
                let finalState = { 0..9 } |> Seq.fold(fun state round -> move round state |> fst) initialState
                let minX, minY, maxX, maxY = getBounds finalState
                (maxX - minX + 1) * (maxY - minY + 1) - (finalState |> List.length)

            let part2 =
                (initialState, 0)
                |> Seq.unfold(fun (state, round) ->
                    match move round state with
                    | (_, false) -> None
                    | (s, true) -> Some (round, (s, round + 1))
                )
                |> Seq.max
                |> (+) 2

            (part1, part2)