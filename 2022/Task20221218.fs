namespace Skrim.AdventOfCode

type Task20221218 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let cubes =
                input
                |> Seq.map(fun line ->
                    let c = line.Split(",") |> Array.map int32
                    (c[0], c[1], c[2])
                )
                |> Set.ofSeq

            let add (x1, y1, z1) (x2, y2, z2) = (x1 + x2, y1 + y2, z1 + z2)
            let neighbors cube = [ (1, 0, 0); (-1, 0, 0); (0, 1, 0); (0, -1, 0); (0, 0, 1); (0, 0, -1) ] |> List.map (fun n -> add cube n)

            let calculateSurface cubeSet =
                let calculateForCube cube =
                    neighbors cube
                    |> List.filter(fun neighbor -> not(cubeSet |> Set.contains neighbor))
                    |> List.length

                cubeSet |> Set.toList |> List.map calculateForCube |> List.sum |> int64

            let fillCavities cubes =
                let (maxX, maxY, maxZ) =
                    let findMax selector = cubes |> Set.map selector |> Set.toList |> List.max |> (+) 2
                    (findMax (fun (x, _, _) -> x), findMax (fun (_, y, _) -> y), findMax (fun (_, _, z) -> z))

                let getWorldIndex (x, y, z) = x + y * maxX + z * maxX * maxY
                let getCoordinates index = ( (index % maxY) % maxZ, (index / maxX) % maxY, (index / maxX / maxY) % maxZ )
                let setNode coords value world = world |> List.updateAt (getWorldIndex coords) value

                let continueFill coords (world:sbyte list) =
                    match coords with
                    | x, y, z when x < 0 || y < 0 || z < 0 || x >= maxX || y >= maxY || z >= maxZ -> false
                    | _ -> world[getWorldIndex coords] = 0y

                let updateNeighbors (world, todo) neighbor =
                    match continueFill neighbor world with
                    | true -> (setNode neighbor 1y world, neighbor::todo)
                    | false -> (world, todo)

                let rec floodFillStep cube (world:sbyte list) =
                    let (newWorld, todo) = cube |> neighbors |> List.fold updateNeighbors (world, List.empty)
                    todo |> List.fold(fun state value -> floodFillStep value state) newWorld

                let emptyWorld = Array.create (maxX * maxY * maxZ) 0y |> Array.toList

                cubes
                |> Set.fold(fun state coords -> setNode coords 2y state) emptyWorld
                |> floodFillStep (0, 0, 0)
                |> List.fold(fun (index, result) value ->
                    match value with
                    | 1y -> (index + 1, result)
                    | _ -> (index + 1, result |> Set.add (getCoordinates index))
                ) (0, Set.empty)
                |> snd

            (cubes |> calculateSurface, cubes |> fillCavities |> calculateSurface)