namespace Skrim.AdventOfCode

open System.Text.RegularExpressions

type Robot = { oreCost : int; clayCost: int; obsidianCost : int; oreProduction : int; clayProduction : int; obsidianProduction : int; geodeProduction : int }
type State = { ore : int; oreDelta : int; clay : int; clayDelta : int; obsidian : int; obsidianDelta : int; geode : int; geodeDelta : int; nextRobot : int }

type Task20221219 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let extractValues s = Regex("-?\\d+").Matches(s) |> Seq.map(fun f -> f.Value |> int32) |> Seq.toList

            let defaultRobot = { oreCost = 0; clayCost = 0; obsidianCost = 0; oreProduction = 0; clayProduction = 0; obsidianProduction = 0; geodeProduction = 0 }
            let defaultState = { ore = 0; oreDelta = 1; clay = 0; clayDelta = 0; obsidian = 0; obsidianDelta = 0; geode = 0; geodeDelta = 0; nextRobot = -1 }

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

            let determineGeodeOutput initialSteps (blueprint:Robot list) =
                let maxOreCost = blueprint |> Seq.map (fun r -> r.oreCost) |> Seq.max
                let maxClayCost = blueprint |> Seq.map (fun r -> r.clayCost) |> Seq.max
                let maxObsidianCost = blueprint |> Seq.map (fun r -> r.obsidianCost) |> Seq.max

                let update s:State =
                    { s with ore = s.ore + s.oreDelta; clay = s.clay + s.clayDelta; obsidian = s.obsidian + s.obsidianDelta; geode = s.geode + s.geodeDelta }

                let canCreateRobot (robot:Robot) (ns:State) = robot.oreCost <= ns.ore && robot.clayCost <= ns.clay && robot.obsidianCost <= ns.obsidian

                let createRobot (robot:Robot) (ns:State) =
                    {
                        ns with
                            ore = ns.ore - robot.oreCost; clay = ns.clay - robot.clayCost; obsidian = ns.obsidian - robot.obsidianCost;
                            oreDelta = ns.oreDelta + robot.oreProduction; clayDelta = ns.clayDelta + robot.clayProduction;
                            obsidianDelta = ns.obsidianDelta + robot.obsidianProduction; geodeDelta = ns.geodeDelta + robot.geodeProduction
                    }

                let hasProductionForRobot (robot:Robot) (ns:State) =
                    (ns.oreDelta > 0 || robot.oreCost = 0) && (ns.clayDelta > 0 || robot.clayCost = 0) && (ns.obsidianDelta > 0 || robot.obsidianCost = 0)

                let rec simulateStep stepsLeft states =

                    let maxGeodes = states |> Set.map (fun v -> v.geode) |> Set.maxElement

                    let discardBranch (robot:Robot) (ns:State) =
                        (ns.geode < maxGeodes) ||
                        (robot.oreProduction > 0 && ns.oreDelta >= maxOreCost) ||
                        (robot.clayProduction > 0 && ns.clayDelta >= maxClayCost) ||
                        (robot.obsidianProduction > 0 && ns.obsidianDelta >= maxObsidianCost) ||
                        (ns.ore > 3 * maxOreCost + ns.oreDelta) || (ns.clay > 3 * maxClayCost + ns.clayDelta) || (ns.obsidian > 2 * maxObsidianCost + ns.obsidianDelta) ||
                        (stepsLeft < initialSteps - 16 && ns.obsidianDelta = 0)

                    let addBranchedSteps stateSet state =
                        let addBranchedRobots nextStep =
                            { 0..3 }
                            |> Seq.filter(fun i ->
                                (i = 3 || not(nextStep |> canCreateRobot blueprint[3])) &&
                                (hasProductionForRobot blueprint[i] state) &&
                                not (discardBranch blueprint[i] nextStep)
                            )
                            |> Seq.fold(fun a v -> a |> Set.add { nextStep with nextRobot = v } ) stateSet

                        match state.nextRobot, state |> update with
                        | nr, updated when nr >= 0 && state |> canCreateRobot blueprint[nr] -> updated |> createRobot blueprint[nr] |> addBranchedRobots
                        | nr, updated when nr < 0 -> updated |> addBranchedRobots
                        | _, updated -> stateSet |> Set.add updated

                    match stepsLeft with
                    | 0 -> maxGeodes
                    | _ -> states |> Set.fold addBranchedSteps Set.empty |> simulateStep (stepsLeft - 1)

                simulateStep initialSteps (Set.singleton defaultState)

            let determineGeodeOutputs steps blueprints = blueprints |> List.map(fun bp -> bp |> determineGeodeOutput steps)

            let part1 = blueprints |> determineGeodeOutputs 24 |> List.fold(fun (index, acc) value -> (index + 1, acc + index * value)) (1, 0) |> snd
            let part2 = blueprints |> List.take 3 |> determineGeodeOutputs 32 |> List.fold (*) 1

            (part1, part2)