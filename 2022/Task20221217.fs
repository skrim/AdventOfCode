namespace Skrim.AdventOfCode

open System
open System.Diagnostics

type CaveState =
    | Empty = 0
    | Falls = 1
    | Solid = 2

type Task20221217 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =

            let entityOfString (str:string) =
                str.Split('|')
                |> Array.map(fun line ->
                    line
                    |> Seq.map (fun c ->
                        match c with
                        | '.' -> CaveState.Empty
                        | '#' -> CaveState.Solid
                        | '@' -> CaveState.Falls
                        | _ -> raise <| new InvalidOperationException()
                    )
                    |> Seq.toList
                )
                |> Array.toList

            let printCave cave =
                for row in cave do
                    for tile in row do
                        match tile with
                        | CaveState.Empty -> printf "."
                        | CaveState.Falls -> printf "@"
                        | CaveState.Solid -> printf "#"
                        | _ -> raise <| new InvalidOperationException()
                    printfn ""

            let rocks = [ "@@@@"; ".@.|@@@|.@."; "..@|..@|@@@"; "@|@|@|@"; "@@|@@" ] |> List.map entityOfString |> List.toArray
            let jets = input |> Seq.head |> Seq.map(fun c -> if c = '<' then -1 else 1) |> Seq.toArray

            let emptyRow = "#.......#" |> entityOfString |> List.head

            let rec expandTo height cave =
                match List.length cave with
                | c when c > height + 1 -> expandTo height (List.tail cave)
                | c when c < height + 1 -> expandTo height (emptyRow :: cave)
                | _ -> cave

            let initialCave = List.empty

            let getEmptyHeight cave =
                let rec getEmptyHeight (cave:CaveState list list) row =
                    //printfn "%A %A" row (cave |> List.head)
                    match cave with
                    | h::_ when h[1..7] |> List.contains CaveState.Solid -> row
                    | _::t -> getEmptyHeight t (row + 1)
                    | [] -> row

                getEmptyHeight cave 0

            let getFirstFullRow cave =
                let rec getFirstFullRow (cave:CaveState list list) row =
                    match cave with
                    | h::_ when h |> List.filter(fun tile -> tile = CaveState.Solid) |> List.length = 9 -> row
                    | _::t -> getFirstFullRow t (row + 1)
                    | [] -> -1

                getFirstFullRow cave 0

            let canPlace (rock:CaveState list list) x y (cave:CaveState list list) =
                rock
                |> List.fold(fun (dy, result) row ->
                    let (_, rowResult) =
                        row
                        |> List.fold(fun (dx, innerResult) tile ->
                            match y + dy with
                            | h when h >= List.length cave -> (dx + 1, false)
                            | h -> (dx + 1, innerResult && (tile = CaveState.Empty || cave[h].[x + dx] = CaveState.Empty))
                        ) (0, result)
                    (dy + 1, rowResult)
                ) (0, true)
                |> snd

            let place rock x y (cave:CaveState list list) =
                rock
                |> List.fold(fun (dy, result) row ->
                    let (_, newCave) =
                        row
                        |> List.fold(fun (dx, innerResult:CaveState list list) tile ->
                            if tile = CaveState.Falls then
                                let newRow = innerResult[y + dy] |> List.updateAt (x + dx) CaveState.Solid
                                (dx + 1, innerResult |> List.updateAt (y + dy) newRow)
                            else
                                (dx + 1, innerResult)
                        ) (0, result)
                    (dy + 1, newCave)
                ) (0, cave)
                |> snd

            let rec fall x y rock jetIndex cave =
                let dx = jets[jetIndex % (jets |> Array.length)]
                let newJetIndex = jetIndex + 1

                let x2 =
                    match canPlace rock (x + dx) y cave with
                    | false -> x
                    | true -> x + dx

                match canPlace rock x2 (y + 1) cave with
                | false ->
                    let v = (place rock x2 y cave, newJetIndex)
                    v
                | true -> fall x2 (y + 1) rock newJetIndex cave

            let dropRock rock jetIndex cave =

                let emptyHeight = getEmptyHeight cave
                let caveHeight = List.length cave
                let rockHeight = List.length rock
                let neededHeight = caveHeight + (rockHeight + 2 - emptyHeight)

                let expandedCave = cave |> expandTo neededHeight
                //printCave expandedCave

                fall 3 0 rock jetIndex expandedCave

            let drop (cave, extraRows, rockIndex, jetPosition) =

                let rock = rocks[rockIndex % (rocks |> Array.length)]
                let (newCave, newJets) = dropRock rock jetPosition cave

                match rockIndex % 10 with
                | 0 ->
                    match getFirstFullRow newCave with
                    | h when h < 0 -> (newCave, extraRows, rockIndex + 1, newJets)
                    | h -> (newCave |> List.take h, extraRows + (((newCave |> List.length) - h) |> int64), rockIndex + 1, newJets)
                | _ -> (newCave, extraRows, rockIndex + 1, newJets)

                //printCave newCave
                //printfn "--"



            let getRockHeight cave extraRows =
                let finalCaveEmpty = getEmptyHeight cave
                (List.length cave) - finalCaveEmpty
                |> int64
                |> (+) extraRows

            let (finalCave, extraRows, _, _) = { 1..2022 } |> Seq.fold(fun state _ -> drop state ) (initialCave, 0L, 0, 0)
            let part1 = getRockHeight finalCave extraRows

            let totalRocks = 1000000000000L
            let cycleLength = Array.length rocks * Array.length jets |> int64
            let cycles = Math.Floor(totalRocks / cycleLength) |> int64
            let remaining = totalRocks % cycleLength

            let mutable prev = 0L

            let (_, (a, b, c)) =
                { 1L..2L }
                //{ 1L..(cycleLength + cycleLength + remaining) * 20L }
                |> Seq.fold(fun (caveState, (a, b, c:int64)) index ->
                    let s = drop caveState
                    let (x, r, _, _) = s
                    if index % cycleLength = 0 then
                        let h = getRockHeight x r
                        printfn "%d %d (%d %d)" index h prev (h - prev)
                        //printCave (x |> List.take 20)
                        prev <- h
                    match s with
                    | (n, r, c, j) when index = cycleLength -> ((n, r, c, j), (getRockHeight n r, b, c))
                    | (n, r, c, j) when index = cycleLength * 2L -> ((n, r, c, j), (a, getRockHeight n r, c))
                    | (n, r, c, j) when index = cycleLength * 2L + remaining -> ((n, r, c, j), (a, b, getRockHeight n r))
                    | result -> (result, (a, b, c))
                ) ((initialCave, 0L, 0, 0), (0L, 0L, 0L))

            printfn "cycleLength %d remaining %d cycles %d a %d b %d c %d" cycleLength remaining cycles a b c

            let part2 = a + (b - a) * (cycles - 1L)

            // rocks 1000000000000

            // 400, h 608
            // 1800, h 2428

            // cycle 1400, h 1820

            // 3200 4848
            // 4600 6968

            // 2120

            (part1, part2)
