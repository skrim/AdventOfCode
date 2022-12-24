namespace Skrim.AdventOfCode

open System
open System.Text.RegularExpressions

type Robot = { oreCost : int; clayCost: int; obsidianCost : int; oreProduction : int; clayProduction : int; obsidianProduction : int; geodeProduction : int }
type State =
    { ore : int; oreDelta : int; clay : int; clayDelta : int; obsidian : int; obsidianDelta : int; geode : int; geodeDelta : int; nextRobot : int;
        //history : State list; createdRobots : int list
    }

type Task20221219 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let extractValues s = Regex("-?\\d+").Matches(s) |> Seq.map(fun f -> f.Value |> int32) |> Seq.toList

            let defaultRobot = { oreCost = 0; clayCost = 0; obsidianCost = 0; oreProduction = 0; clayProduction = 0; obsidianProduction = 0; geodeProduction = 0 }
            let defaultState =
                {
                    ore = 0; oreDelta = 1; clay = 0; clayDelta = 0; obsidian = 0; obsidianDelta = 0; geode = 0; geodeDelta = 0; nextRobot = -1;
                    //history = []; createdRobots = []
                }

            let blueprints =
                input
                |> Seq.map extractValues
                |> Seq.map (fun values ->
                    [
                        { defaultRobot with oreCost = values[1]; oreProduction = 1 };
                        { defaultRobot with oreCost = values[2]; clayProduction = 1 };
                        { defaultRobot with oreCost = values[3]; clayCost = values[4]; obsidianProduction = 1 };
                        { defaultRobot with oreCost = values[5]; obsidianCost = values[6]; geodeProduction = 1 };
                    ]
                )
                |> Seq.toList

            let printState (s:State) =
                printfn "ore %2d %2d clay %2d %2d obs %2d %2d geod %2d %2d next %d %A"
                    s.ore s.oreDelta s.clay s.clayDelta s.obsidian s.obsidianDelta s.geode s.geodeDelta s.nextRobot
                    //(s.createdRobots |> List.rev)

            let simulate (blueprint:Robot list) =

                //for d in blueprint do printfn "%A" d

                let maxOreCost = blueprint |> Seq.map (fun r -> r.oreCost) |> Seq.max
                let maxClayCost = blueprint |> Seq.map (fun r -> r.clayCost) |> Seq.max
                let maxObsidianCost = blueprint |> Seq.map (fun r -> r.obsidianCost) |> Seq.max

                let update s:State =
                    {
                        s with ore = s.ore + s.oreDelta; clay = s.clay + s.clayDelta; obsidian = s.obsidian + s.obsidianDelta; geode = s.geode + s.geodeDelta;
                            //history = { s with history = [] }::s.history
                    }

                let canCreateRobot (robot:Robot) (ns:State) =
                    robot.oreCost <= ns.ore && robot.clayCost <= ns.clay && robot.obsidianCost <= ns.obsidian

                let createRobot robotIndex (robot:Robot) (ns:State) =
                    {
                        ns with
                            ore = ns.ore - robot.oreCost; clay = ns.clay - robot.clayCost; obsidian = ns.obsidian - robot.obsidianCost;
                            oreDelta = ns.oreDelta + robot.oreProduction; clayDelta = ns.clayDelta + robot.clayProduction;
                            obsidianDelta = ns.obsidianDelta + robot.obsidianProduction; geodeDelta = ns.geodeDelta + robot.geodeProduction;
                            //createdRobots = robotIndex :: ns.createdRobots
                    }

                let hasProductionForRobot (robot:Robot) (ns:State) =
                    (ns.oreDelta > 0 || robot.oreCost = 0) && (ns.clayDelta > 0 || robot.clayCost = 0) && (ns.obsidianDelta > 0 || robot.obsidianCost = 0)

                let hasMaxedOut steps (robot:Robot) (ns:State) =
                    (robot.oreProduction > 0 && ns.oreDelta >= maxOreCost) ||
                    (robot.clayProduction > 0 && ns.clayDelta >= maxClayCost) ||
                    (robot.obsidianProduction > 0 && ns.obsidianDelta >= maxObsidianCost) ||
                    (ns.ore > 3 * maxOreCost + ns.oreDelta) || (ns.clay > 3 * maxClayCost + ns.clayDelta) || (ns.obsidian > 3 * maxObsidianCost + ns.obsidianDelta) ||
                    (steps < 11 && ns.obsidianDelta = 0)

                let rec simulateStep2 count (states:Set<State>) (allStates:Set<State>)=
                    if count = 1 then
                        let max = states |> Set.toSeq |> Seq.maxBy(fun v -> v.geode)

(*
                        let allMax =
                            states
                            |> Set.filter (fun f -> f.geode = max.geode)

                        for s in allMax do
                            printfn "---"
                            (s :: s.history) |> List.rev |> List.mapi (fun i v ->
                                printf "%d: " i
                                printState v
                            )
                            |> ignore

                            *)

                        max.geode
                    else

                        let (nextStates, nextAll) =
                            states |>
                            Set.fold(fun (acc, allAcc) s ->

                                if s.nextRobot < 0 || s |> canCreateRobot blueprint[s.nextRobot] then
                                    let ns =
                                        match s.nextRobot with
                                        | -1 -> s |> update
                                        | _ -> s |> update |> createRobot s.nextRobot blueprint[s.nextRobot]

                                    {0..3}
                                    |> Seq.filter(fun i -> hasProductionForRobot blueprint[i] s && not(hasMaxedOut count blueprint[i] ns))
                                    |> Seq.fold(fun (a, allSt) v ->
                                        let candidate = { ns with nextRobot = v }
                                        match allSt |> Set.contains candidate with
                                        | false -> (a |> Set.add candidate, allSt |> Set.add candidate)
                                        | true -> (a, allSt)
                                    ) (acc, allAcc)
                                else
                                    let nx = s |> update
                                    (acc |> Set.add nx, allAcc)
                            ) (Set.empty, allStates)


                        //for d in nextStates do printfn "%d %A" count d

                        (*

                        let oreFiltered =
                            nextStates
                            |> Set.fold(fun state value ->
                                let key = { value with ore = 0 }
                                if state |> Map.containsKey key then
                                    let existing =
                                else
                                    state |> Map.add key value
                            ) Map.empty

                            *)



                        let mo = nextStates |> Set.map(fun v -> v.obsidian) |> Set.maxElement
                        let mg = nextStates |> Set.map(fun v -> v.geode) |> Set.maxElement

                        printfn "steps left %d, states %d, all %d, max obsidian %d, max geode %d" count (nextStates |> Set.count) (nextAll |> Set.count) mo mg

                        simulateStep2 (count - 1) nextStates nextAll


                //simulateStep defaultState
                simulateStep2 33 (Set.singleton defaultState) Set.empty

            let part1 =
                blueprints
                |> List.take 3
                |> List.mapi(fun index bp ->
                    let id = index + 1
                    let gc = bp |> simulate
                    printfn "%d %d %d" id gc (id * gc)
                    id * gc
                )
                |> List.sum
            // 1136 low

            (part1, 0)
