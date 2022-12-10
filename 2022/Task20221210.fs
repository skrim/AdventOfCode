namespace Skrim.AdventOfCode

open System

type Task20221210 () =
    interface IAdventOfCodeTask<int64, string> with
        override x.Solve(input) =
            let states =
                input
                |> Seq.fold(fun (history, current) value ->
                    match value.Split(' ') |> Array.toList with
                    | "noop" :: [] -> (current :: history, current)
                    | "addx" :: b :: [] ->
                        let next = current + (b |> int32)
                        (next :: current :: history, next)
                    | _ -> raise <| new InvalidOperationException()
                ) (List.empty, 1)
                |> fst |> List.tail |> List.rev

            let formatOutput pixels =
                pixels
                |> List.fold(fun (output, index) value ->
                    (
                        (if value then "#" else ".")
                        :: (if (index > 0 && index % 40 = 0) then "\n" else "")
                        :: output,
                    index + 1)
                ) (List.empty, 0)
                |> fst |> List.rev |> String.concat ""

            let pixels =
                (1 :: states)
                |> List.fold(fun (pixels, index) x ->
                    ((abs (index % 40 - x) <= 1) :: pixels, index + 1)
                ) (List.empty, 0)
                |> fst |> List.rev

            let calculateStrength index = index * states[index - 2]

            let part1 = {20..40..220} |> Seq.map calculateStrength |> Seq.sum
            let part2 = formatOutput pixels

            (part1, part2)