namespace Skrim.AdventOfCode

open System

type Task20221206 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =

            let findMatch characters count =
                let rec findInternal characters count queue pos =
                    match characters with
                    | head :: remaining ->
                        let distinct = (Set.ofList queue).Add(head)
                        if Set.count distinct = count then
                            pos + 1
                        elif List.length queue < count - 1 then
                            findInternal remaining count (head :: queue) (pos + 1)
                        else
                            findInternal remaining count (head :: (queue |> List.rev |> List.tail |> List.rev)) (pos + 1)
                    | [] -> raise <| new InvalidOperationException()

                findInternal characters count List.empty 0

            let data = input |> Seq.head |> List.ofSeq

            (findMatch data 4, findMatch data 14)