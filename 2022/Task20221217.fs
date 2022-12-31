namespace Skrim.AdventOfCode

type Task20221217 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =

            let entityOfString (s:string) = s.Split('|') |> Array.map(fun l -> l |> Seq.map(fun c -> c <> '.') |> Seq.toList ) |> Array.toList

            let rocks = [ "@@@@"; ".@.|@@@|.@."; "..@|..@|@@@"; "@|@|@|@"; "@@|@@" ] |> List.map entityOfString
            let jets = input |> Seq.head |> Seq.map(fun c -> if c = '<' then -1 else 1) |> Seq.toList

            let rockLength, jetLength = rocks |> List.length |> int64, jets |> List.length |> int64

            let emptyRow = "#.......#" |> entityOfString |> List.head
            let fullRow = "#########" |> entityOfString |> List.head

            let findRow cave row matcher =
                let rec findRowStep index = function
                    | h::_ when matcher h row -> index
                    | _::t -> findRowStep (index + 1) t
                    | [] -> index

                findRowStep 0 cave

            let getEmptyHeight cave = findRow cave emptyRow (<>)
            let getFirstFullRow cave = findRow cave fullRow (=)

            let drop (sourceCave, extraRows, rockIndex, jetIndex) =
                let rock = rocks[ (rockIndex % rockLength) |> int32 ]
                let rockWidth, rockHeight = (rock |> List.head |> List.length, rock |> List.length)

                let concatArea x y xc yc list = list |> List.skip y |> List.truncate yc |> List.map (fun r -> r |> List.skip x |> List.truncate xc) |> List.concat

                let mapRange fromIndex count method list =
                    let matchInRange index item =
                        match index >= fromIndex && index < fromIndex + count with
                        | true -> method index item
                        | false -> item
                    list |> List.mapi matchInRange

                let place tx ty cave = cave |> mapRange ty rockHeight (fun y row -> row |> mapRange tx rockWidth (fun x tile -> tile || rock[y - ty].[x - tx]))

                let canPlace x y cave =
                    match x >= 1 && y >= 0 && x + rockWidth < 9 && y + rockHeight <= (cave |> List.length) with
                    | false -> false
                    | true -> (rock |> List.concat, cave |> concatArea x y rockWidth rockHeight) ||> List.fold2 (fun s v1 v2 -> s && not (v1 && v2)) true

                let rec fall x y jetIndex cave =
                    let dx = jets[ (jetIndex % jetLength) |> int32 ]
                    let x2 = x + dx * (if canPlace (x + dx) y cave then 1 else 0)

                    match canPlace x2 (y + 1) cave with
                    | false -> (place x2 y cave, jetIndex + 1L)
                    | true -> fall x2 (y + 1) (jetIndex + 1L) cave

                let rec adjustLength height cave =
                    match List.length cave with
                    | c when c > height + 1 -> adjustLength height (List.tail cave)
                    | c when c < height + 1 -> adjustLength height (emptyRow :: cave)
                    | _ -> cave

                let dropRock jetIndex cave =
                    let emptyHeight, caveHeight, rockHeight = getEmptyHeight cave, List.length cave, List.length rock
                    let neededHeight = caveHeight + (rockHeight + 2 - emptyHeight)

                    fall 3 0 jetIndex (cave |> adjustLength neededHeight)

                let (newCave, newJets) = dropRock jetIndex sourceCave

                match getFirstFullRow newCave with
                | h when h < 0 -> (newCave, extraRows, rockIndex + 1L, newJets)
                | h -> (newCave |> List.take h, extraRows + (((newCave |> List.length) - h) |> int64), rockIndex + 1L, newJets)

            let getRockHeight cave extraRows = (List.length cave) - (getEmptyHeight cave) |> int64 |> (+) extraRows

            let result1 = { 1..2022 } |> Seq.fold(fun state _ -> drop state ) (List.empty, 0L, 0, 0)
            let (finalCave, extraRows, rockIndex, jetIndex) = result1
            let part1 = getRockHeight finalCave extraRows

            let part2 =
                let findCycle state =
                    match drop state with
                    | (_, _, ri2, jp2) when ri2 % rockLength = rockIndex % rockLength && jetIndex % jetLength = jp2 % jetLength -> None
                    | state2 -> Some (state2, state2)

                let (cave2, exraRows2, rockIndex2, jetIndex2) = result1 |> Seq.unfold findCycle  |> Seq.rev |> Seq.head |> drop

                let cycleLength, cycleHeight = rockIndex2 - rockIndex, (getRockHeight cave2 exraRows2) - part1
                let totalRocks = 1000000000000L
                let cycles = (totalRocks - rockIndex2) / cycleLength
                let rocksUsed = rockIndex2 + cycles * cycleLength

                let result2 = { rocksUsed + 1L .. totalRocks } |> Seq.fold(fun state _ -> drop state) (cave2, exraRows2 + cycleHeight * cycles, rockIndex2, jetIndex2)
                let (finalCave2, extraRows2, _, _) = result2
                getRockHeight finalCave2 extraRows2

            (part1, part2)