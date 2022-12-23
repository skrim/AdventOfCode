namespace Skrim.AdventOfCode
open System
open System.Text.RegularExpressions

type MapNode =
    | Void
    | Floor
    | Wall

type Step =
    | Left
    | Right
    | Forward

type Task20221222 () =

    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let height = (Seq.length input) - 2
            let width = input |> Seq.take height |> Seq.map String.length |> Seq.max

            let parseData input =
                let expandVoid length list =
                    match list |> List.length with
                    | l when l >= length -> list
                    | l -> list @ ((Array.create (length - l) Void) |> Array.toList)

                let parseNode = function
                    | ' ' -> Void
                    | '.' -> Floor
                    | '#' -> Wall
                    | _ -> raise <| new InvalidOperationException()

                let parseDirection = function
                    | "R" -> seq { Right }
                    | "L" -> seq { Left }
                    | v -> Array.create (v |> int32) Forward

                let createTrack directions =
                    Regex("(\\d+|R|L)").Matches(directions) |> Seq.map(fun f -> parseDirection f.Value) |> Seq.concat

                let parseLine line = line |> Seq.map parseNode |> Seq.toList |> expandVoid width

                (input |> Seq.take height |> Seq.map parseLine |> Seq.toList, input |> Seq.rev |> Seq.head |> createTrack)

            let orientations = [ (1, 0); (0, 1); (-1, 0); (0, -1) ]

            let rotate current turn =
                let index = orientations |> List.findIndex (fun v -> v = current)
                orientations[ (index + turn + 4) % 4 ]

            let map, moves = input |> parseData
            let start = (map[0] |> List.findIndex(fun f -> f = Floor), 0)

            let planeMove (startX, startY) (dx, dy) =
                let rec getStep (px, py) (cx, cy) =
                    let tx = (px + cx + width) % width
                    let ty = (py + cy + height) % height
                    match map[ty].[tx] with
                    | Floor -> (tx, ty)
                    | Wall -> (startX, startY)
                    | Void -> getStep (px, py) (cx + dx, cy + dy)

                let newPos = getStep (startX, startY) (dx, dy)
                (newPos, (dx, dy))

            let cubeMove (sx, sy) orientation =
                let (dx, dy) = orientation

                let edgeCrossings = [
                        ( (50, 0, 0), (0, 199, 3) );
                        ( (50, 49, 3), (0, 149, 3) );
                        ( (100, 0, 0), (49, 199, 2) );
                        ( (149, 0, 1), (99, 100, 1) );
                        ( (99, 50, 1), (149, 49, 2) );
                        ( (50, 99, 3), (0, 100, 0) );
                        ( (49, 150, 1), (99, 149, 2) )
                    ]

                let onEdge (ex, ey, eo) =
                    let (eox, eoy) = orientations[eo];
                    let xr, yr = [ex; ex + (eox * 49)] |> List.sort, [ey; ey + (eoy * 49)] |> List.sort
                    rotate (eox, eoy) -1 = orientation && sx >= xr[0] && sx <= xr[1] && sy >= yr[0] && sy <= yr[1]

                let findEdge state (edge1, edge2) =
                    match state with
                    | None when onEdge edge1 -> Some (edge1, edge2)
                    | None when onEdge edge2 -> Some (edge2, edge1)
                    | s -> s

                let crossover (fx, fy, _) (tx, ty, toOrientation) =
                    let distanceFromCorner = [ fx - sx; fy - sy ] |> List.map (fun v -> Math.Abs(v)) |> List.max
                    let (ox, oy) = orientations[toOrientation]
                    ( ( tx + (49 - distanceFromCorner) * ox, ty + (49 - distanceFromCorner) * oy ), rotate (ox, oy) 1 )

                let ((newX, newY), newOrientation) =
                    match edgeCrossings |> List.fold findEdge None with
                    | None -> (sx + dx, sy + dy), orientation
                    | Some (fromEdge, toEdge) -> crossover fromEdge toEdge

                match map[newY].[newX] with
                | Floor -> ((newX, newY), newOrientation)
                | Wall -> ((sx, sy), orientation)
                | _ -> raise <| new InvalidOperationException()

            let traverse moveAction =
                let doMove (position, orientation) = function
                    | Right -> (position, rotate orientation 1)
                    | Left -> (position, rotate orientation -1)
                    | Forward -> moveAction position orientation

                let ((gx, gy), finalOrientation) = moves |> Seq.fold doMove ( start, (1, 0))

                4 * (gx + 1) + 1000 * (gy + 1) + (orientations |> List.findIndex(fun v -> v = finalOrientation))

            (traverse planeMove, traverse cubeMove)