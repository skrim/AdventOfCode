namespace Skrim.AdventOfCode

open System

type Task20221212 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =

            let (grid, start, goal, width, height, MAXDISTANCE) =
                let (gr, _, start, goal) =
                    input
                    |> Seq.fold(fun (state, y, st, en) line ->
                        let (row, _, s, e) =
                            line
                            |> Seq.fold(fun (state, x, st2, en2) tile ->
                                match tile with
                                | 'S' -> (0 :: state, x + 1, (x, y), en2)
                                | 'E' -> (25 :: state, x + 1, st2, (x, y))
                                | t -> ((t |> int) - 97 :: state, x + 1, st2, en2)
                            ) (List.empty, 0, st, en)
                        ((row |> List.rev) :: state, y + 1, s, e)
                    ) (List.empty, 0, (-1, -1), (-1, -1))

                (gr |> List.rev, start, goal, gr |> List.head |> List.length, gr |> List.length, 100000)

            let foldGrid func init =
                { 0 .. height - 1}
                |> Seq.fold(fun state y ->
                    { 0 .. width - 1 } |> Seq.fold(fun innerState x -> innerState |> func x y grid[y].[x] ) state
                ) init

            let priorityInsert (item:int*int) (priority:int) (queue:List<int*(int*int)>) =
                let element = (priority, item)
                let rec finish accumulation = function
                    | [] -> accumulation
                    | h::t -> finish (h :: accumulation) t
                let rec insertStep accumulation = function
                    | [] -> List.rev (element :: accumulation)
                    | (p, i)::t when p > priority -> finish (element :: (p, i) :: t) accumulation
                    | i::t -> insertStep (i :: accumulation) t
                insertStep [] queue

            let findRoute start goal cost =
                let getHeight (gx, gy) = grid[gy].[gx]

                let neighbors (x, y) =
                    let findNeighbor list (dx, dy) =
                        match (x + dx, y + dy) with
                        | tx, ty when tx < 0 || ty < 0 || tx >= width || ty >= height -> list
                        | tx, ty ->
                            match cost (getHeight (tx, ty)) (getHeight (x, y)) with
                            | -1 -> list
                            | c -> (tx, ty, c) :: list

                    [ (-1, 0); (1, 0); (0, -1); (0, 1) ] |> List.fold findNeighbor List.empty

                let rec findRouteStep (openDistances:List<int*(int*int)>) (distances:Map<int*int, int>) (previous:Map<int*int, int*int>) =
                    let unfoldPath current =
                        match previous |> Map.containsKey current with
                        | true -> Some ((current, cost (getHeight current) (getHeight (previous |> Map.find current))), previous |> Map.find current)
                        | false -> None

                    match openDistances |> List.head with
                    | (_, smallest) when smallest = goal -> smallest |> List.unfold unfoldPath |> List.rev
                    | (smallestDist, _) when smallestDist = MAXDISTANCE -> raise <| new InvalidOperationException()
                    | (_, smallest) ->
                        let nextNodes = openDistances |> List.tail

                        let foldNeighbors (openDist, dist, prev) (nx, ny, cost) =
                            let alt = (dist |> Map.find smallest) + cost
                            match alt < (dist |> Map.find (nx, ny)) with
                            | true -> (openDist |> priorityInsert (nx, ny) alt, dist |> Map.add (nx, ny) alt, prev |> Map.add (nx, ny) smallest)
                            | false -> (openDist, dist, prev)

                        let (newOpenDist, newDistances, newPrev) = smallest |> neighbors |> List.fold foldNeighbors (nextNodes, distances, previous)

                        findRouteStep newOpenDist newDistances newPrev

                let previous = Map.empty
                let openDistances = foldGrid(fun x y _ state -> state |> priorityInsert (x, y) (if (x, y) = start then 0 else MAXDISTANCE) ) List.empty
                let distances = foldGrid (fun x y _ state -> state |> Map.add (x, y) (if (x, y) = start then 0 else MAXDISTANCE) ) Map.empty
                findRouteStep openDistances distances previous

            let costPart1 h1 h2 =
                match true with
                | _ when h1 - h2 > 1 -> -1
                | _ -> 1

            let costPart2 h1 h2 =
                match h1, h2 with
                | 0, 0 -> 0
                | _ when h1 - h2 > 1 -> -1
                | _ -> 1

            let calculate costFunction = (findRoute start goal costFunction) |> Seq.map snd |> Seq.sum |> int64

            (calculate costPart1, calculate costPart2)