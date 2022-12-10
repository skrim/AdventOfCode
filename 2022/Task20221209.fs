namespace Skrim.AdventOfCode

open System

type Task20221209 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let moves =
                input
                |> Seq.map(fun f ->
                    let parts = f.Split(' ')
                    ( parts[0], parts[1] |> int32)
                )

            let pullRope length =
                let moveSegment (previous, newRope) next =
                    let followPrevious op =
                        let n = op next
                        match op previous with
                        | p when n > p -> n - 1
                        | p when n < p -> n + 1
                        | _ -> n

                    let needsMove op = abs (op next - op previous) > 1

                    let newSegment = if needsMove fst || needsMove snd then (followPrevious fst, followPrevious snd) else next

                    (newSegment, newSegment :: newRope)

                let (_, positions) =
                    moves
                    |> Seq.fold(fun state (direction, count) ->
                        { 1..count }
                        |> Seq.fold(fun (rope, tailPositions:Set<int*int>) _ ->
                            match rope with
                            | head :: tail ->
                                let newHead =
                                    match direction with
                                    | "U" -> (fst head, snd head + 1)
                                    | "D" -> (fst head, snd head - 1)
                                    | "L" -> (fst head - 1, snd head)
                                    | "R" -> (fst head + 1, snd head)
                                    | _ -> raise <| new InvalidOperationException()

                                let (_, newRope) = tail |> List.fold moveSegment (newHead, [ newHead ])
                                (newRope |> List.rev, tailPositions.Add(newRope |> List.head))
                            | _ -> raise <| new InvalidOperationException()

                        ) state
                    ) (Array.create length (0, 0) |> Array.toList, Set.empty)

                positions |> Set.count

            (pullRope 2, pullRope 10)