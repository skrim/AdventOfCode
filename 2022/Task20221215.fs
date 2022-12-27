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

            let minManhattanDistance = sensorsAndBeacons |> List.map (fun (_, _, md) -> md) |> List.min

            let xRange y (sx:int64, sy:int64, md:int64) =
                let yd = md - Math.Abs(sy - y)
                (sx - yd, sx + yd)

            let yRange x (sx:int64, sy:int64, md:int64) = xRange x (sy, sx, md)

            let cull (a1, a2) = (Math.Max(0L, Math.Min(areaSize, a1)), Math.Max(0L, Math.Min(areaSize, a2)))

            let rec mergeToList list item =
                match item, list with
                | _, [] -> item :: List.empty
                | (a1, a2), (b1, b2) :: tail when a2 < b1 - 1L || a1 > b2 + 1L -> (b1, b2) :: (mergeToList tail item)
                | (a1, a2), (b1, b2) :: tail -> mergeToList tail (Math.Min(a1, b1), Math.Max(a2, b2))

            let calculateOverlap (ca, cb) list =
                list
                |> List.fold (fun state item ->
                    match item with
                    | oa, ob when cb < oa || ca > ob -> state
                    | oa, ob when ca <= oa && ob >= ob -> state + (cb - ca + 1L)
                    | oa, _ when ca < oa -> state + (cb - oa + 1L)
                    | _, ob -> state + (ob - ca + 1L)
                ) 0L

            let ranges accessor p =
                sensorsAndBeacons
                |> List.map (fun r -> r |> accessor p)
                |> List.filter (fun (a, b) -> a <= b)
                |> List.sortBy (fun (a, _) -> a)

            let find accessor =
                let calculate p =
                    let (mergedRanges, minOverlap) =
                        ranges accessor p
                        |> List.fold(fun (merged, minOverlap) range ->
                            match merged with
                            | [] -> (mergeToList merged range, minOverlap)
                            | _ -> (mergeToList merged range, Math.Min(merged |> calculateOverlap range, minOverlap))
                        ) (List.empty, areaSize)

                    let filled = mergedRanges |> List.map cull |> List.map (fun (a, b) -> b - a) |> List.sum
                    (minOverlap, filled)

                let rec findStep p =
                    let (minOverlap, filled) = calculate p

                    match filled with
                    | _ when filled = areaSize - 2L -> p
                    | _ when p >= areaSize - 1L -> raise <| new InvalidOperationException()
                    | _ when minOverlap <= 1L && (calculate (p + minManhattanDistance) |> fst) = minOverlap -> findStep (p + minManhattanDistance)
                    | _ when minOverlap > 1L -> findStep (p + minOverlap / 2L)
                    | _ -> findStep (p + 1L)

                findStep 0L

            let part1 = yPos |> ranges xRange |> List.fold mergeToList List.empty |> List.map (fun (a, b) -> b - a) |> List.sum

            // 3299359 3355220
            let yp = find xRange
            let xp = find yRange
            printfn "%d %d" xp yp
            let part2 = xp * 4000000L + yp

            (part1, part2)