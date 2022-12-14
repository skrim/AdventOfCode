namespace Skrim.AdventOfCode

open System

type TileState =
    | Empty = 0
    | Wall = 1
    | Sand = 2

type Task20221214 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let paths =
                input
                |> Seq.map(fun line ->
                    line.Split(" -> ")
                    |> Array.map(fun coordinatePair ->
                        let c = coordinatePair.Split(',')
                        (c[0] |> int32, c[1] |> int32)
                    )
                )
                |> Seq.toArray

            let maxY = paths |> Array.reduce Array.append |> Array.map snd |> Array.max

            let widthNeeded = maxY + 3
            let minX = 500 - widthNeeded
            let maxX = 500 + widthNeeded
            let dropPoint = (501 - minX, 0)

            let fill (x, y) v grid =
                let newRow = grid |> List.item y |> List.updateAt x v
                grid |> List.updateAt y newRow

            let rec drawLine source target v grid =
                let newGrid = grid |> fill source v
                match source, target with
                | _, _ when source = target -> newGrid
                | (x1, y1), (x2, y2) -> drawLine (x1 + Math.Sign(x2 - x1), y1 + Math.Sign(y2 - y1)) target v newGrid

            let emptyGrid = Array.create (maxY + 3) (Array.create (maxX - minX + 3) TileState.Empty |> Array.toList) |> Array.toList

            let grid =
                paths
                |> Array.append [| [| (minX - 1, maxY + 2); (maxX + 1, maxY + 2) |] |]
                |> Array.fold(fun outerState path ->
                    path
                    |> Array.map(fun (x, y) -> (x - minX + 1, y))
                    |> Array.fold(fun (state, prev) next ->
                        if fst prev = -1 then
                            (state, next)
                        else
                            (drawLine prev next TileState.Wall state, next)
                    ) (outerState, (-1, -1))
                    |> fst
                ) emptyGrid

            let run endCondition grid =
                let rec fall sand (grid:TileState list list) =
                    match sand with
                    | (_, _) when endCondition grid sand -> (grid, true)
                    | (sx, sy) when grid[sy + 1].[sx] = TileState.Empty -> grid |> fall (sx, sy + 1)
                    | (sx, sy) when grid[sy + 1].[sx - 1] = TileState.Empty -> grid |> fall (sx - 1, sy + 1)
                    | (sx, sy) when grid[sy + 1].[sx + 1] = TileState.Empty -> grid |> fall (sx + 1, sy + 1)
                    | (_, _) -> (grid |> fill sand TileState.Sand, false)

                let rec nextGrain count grid =
                    match grid |> fall dropPoint with
                    | (nextGrid, false) -> nextGrain (count + 1) nextGrid
                    | (_, true) -> count

                grid |> nextGrain 0

            let part1 = grid |> run (fun _ (_, y) -> y = maxY)
            let part2 = grid |> run (fun g _ -> g[snd dropPoint].[fst dropPoint] = TileState.Sand)
            (part1, part2)