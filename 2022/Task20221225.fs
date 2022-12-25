namespace Skrim.AdventOfCode

open System

type Task20221225 () =

    interface IAdventOfCodeTask<string> with
        override x.Solve(input) =
            let decode value =
                let parseToken = function
                    | '=' -> -2L
                    | '-' -> -1L
                    | '0' -> 0L
                    | '1' -> 1L
                    | '2' -> 2L
                    | _ -> raise <| new InvalidOperationException()

                let rec parseStep str m =
                    match str with
                    | [] -> 0L
                    | h::t -> m * (parseToken h) + (parseStep t (m * 5L))

                parseStep (value |> Seq.toList |> List.rev) 1L

            let encode value =
                let encodeToken v =
                    match (v % 5L) - 2L with
                    | -2L -> "="
                    | -1L -> "-"
                    | 0L -> "0"
                    | 1L -> "1"
                    | 2L -> "2"
                    | _ -> raise <| new InvalidOperationException()

                let rec encodeStep v =
                    let t = v + 2L
                    let r = encodeToken t
                    match t / 5L with
                    | 0L -> r
                    | n -> (encodeStep n) +  r

                encodeStep value

            let part1 = input |> Seq.map decode |> Seq.sum |> encode
            (part1, "")