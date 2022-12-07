namespace Skrim.AdventOfCode

open System

type Task20221203 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let score c =
                match c with
                | c when c >= 'a' && c <= 'z' -> int32 c - 96
                | c when c >= 'A' && c <= 'Z' -> int32 c - 38
                | _ -> raise <| new InvalidOperationException()

            let part1 =
                input
                |> Seq.map (fun f ->
                    let halfLength = f.Length / 2
                    Set.intersect (Set.ofSeq f[..halfLength - 1]) (Set.ofSeq f[halfLength..])
                    |> Set.minElement
                    |> score
                )
                |> Seq.sum

            let part2 =
                input
                |> Seq.splitInto ((Seq.length input) / 3)
                |> Seq.map (fun f ->
                    (Set.ofSeq f[0])
                    |> Set.intersect (Set.ofSeq f[1])
                    |> Set.intersect (Set.ofSeq f[2])
                    |> Set.minElement
                    |> score
                )
                |> Seq.sum

            (part1, part2)