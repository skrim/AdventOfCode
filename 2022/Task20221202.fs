namespace Skrim.AdventOfCode

open System

type Task20221202 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let convert x =
                match x with
                | "A" | "X" -> 0
                | "B" | "Y" -> 1
                | "C" | "Z" -> 2
                | _ -> raise <| new InvalidOperationException()

            let calculateScore games =
                games
                |> Seq.map (fun v ->
                    let victory =
                        match v with
                        | (a, b) when a = b -> 3
                        | (a, b) when a = b - 1 || a = b + 2 -> 6
                        | _ -> 0
                    victory + snd v + 1
                )
                |> Seq.sum

            let games = input |> Seq.map (fun f -> f.Split(' ')) |> Seq.map (fun f -> (convert f[0], convert f[1]))

            let score1 = calculateScore games

            let score2 =
                games
                |> Seq.map (fun v ->
                    match v with
                    | (a, b) when b = 0 -> (a, (a + 2) % 3)
                    | (a, b) when b = 2 -> (a, (a + 1) % 3)
                    | (a, _) -> (a, a)
                )
                |> calculateScore

            (score1, score2)