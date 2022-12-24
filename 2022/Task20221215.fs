namespace Skrim.AdventOfCode

open System
open System.Text.RegularExpressions

type Task20221215 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let yPos = input |> Seq.head |> int64
            let areaSize = yPos * 2L

            let extractValues s = Regex("-?\\d+").Matches(s) |> Seq.map(fun f -> f.Value |> int64)

            let sensorsAndBeacons =
                input
                |> Seq.tail
                |> Seq.map extractValues
                |> Seq.map Seq.toList
                |> Seq.map(fun s -> (s[0], s[1], Math.Abs(s[0] - s[2]) + Math.Abs(s[1] - s[3])))
                |> Seq.toList

            let xRange y (sx:int64, sy:int64, md: int64) =
                let yd = md - Math.Abs(sy - y)
                (sx - yd, sx + yd)

            let yRange x (sx:int64, sy:int64, md:int64) = xRange x (sy, sx, md)

            let cull (a1, a2) = (Math.Max(0L, Math.Min(areaSize, a1)), Math.Max(0L, Math.Min(areaSize, a2)))

            let ranges transform yp =
                let rec mergeToList item list =
                    match item, list with
                    | _, [] -> item :: List.empty
                    | (a1, a2), (b1, b2) :: tail when a2 < b1 - 1L || a1 > b2 + 1L -> (b1, b2) :: (tail |> mergeToList item)
                    | (a1, a2), (b1, b2) :: tail -> tail |> mergeToList (Math.Min(a1, b1), Math.Max(a2, b2))

                sensorsAndBeacons
                |> List.fold(fun state value ->
                    match value |> transform yp with
                    | (a, b) when a < b -> state |> mergeToList (a, b)
                    | _ -> state
                ) List.empty

            let part1 = yPos |> ranges xRange |> List.map (fun (a, b) -> b - a) |> List.sum

            let find accessor =
                { 0L..areaSize }
                |> Seq.find(fun p ->
                    let v = p |> ranges accessor |> List.map cull |> List.map (fun (a, b) -> b - a) |> List.sum
                    v = areaSize - 2L
                )

            (0, 0)

            // 3299359 3355220
            //let yp = find xRange
            //let xp = find yRange
            //printfn "%d %d" xp yp
            //let part2 = xp * 4000000L + yp

            //(part1, part2)
