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

                let followPrevious next previous =
                    match previous with
                    | _ when next = previous -> next
                    | _ when next > previous -> next - 1
                    | _ -> next + 1

                let move direction segment =
                    match direction with
                    | "U" -> (fst segment, snd segment + 1)
                    | "D" -> (fst segment, snd segment - 1)
                    | "L" -> (fst segment - 1, snd segment)
                    | "R" -> (fst segment + 1, snd segment)
                    | _ -> raise <| new InvalidOperationException()

                let (_, positions) =
                    moves
                    |> Seq.fold(fun state (direction, count) ->
                        [| 1..count |]
                        |> Array.fold(fun (rope, positions:Set<int*int>) _ ->

                            let newHead = rope |> Array.head |> move direction

                            let newRope =
                                rope
                                |> Array.tail
                                |> Array.fold(fun (previous, newRope) next  ->
                                    let newSegment =
                                        if abs (fst next - fst previous) > 1 || abs (snd next - snd previous) > 1 then
                                            (followPrevious (fst next) (fst previous), followPrevious (snd next) (snd previous))
                                        else
                                            next

                                    (newSegment, Array.append newRope [| newSegment |])
                                ) (newHead, [| newHead |])
                                |> snd

                            (newRope, positions.Add(newRope |> Array.rev |> Array.head))
                        ) state
                    ) (Array.create length (0, 0), Set.empty)

                positions |> Set.count

            (pullRope 2, pullRope 10)
