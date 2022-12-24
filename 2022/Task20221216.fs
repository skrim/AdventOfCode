namespace Skrim.AdventOfCode

open System
open System.Text.RegularExpressions

type Task20221216 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let extractValues s =
                let m = Regex("^Valve (\\w\\w) has flow rate=(\\d+); tunnels? leads? to valves? (?:(\\w\\w)(?:\\,\\s)?)+").Match(s)
                match m.Groups |> Seq.toList with
                | _::id::rate::connections::[] -> (id.Value, (rate.Value |> int32, connections.Captures |> Seq.toList |> List.map string))
                | _ -> raise <| new InvalidOperationException()

            let network = input |> Seq.map extractValues |> Map.ofSeq

            let run count actorPositions =
                let cull = 100

                let rec nextStates current =
                    let mergeSimultaneousMove current =
                        match current with
                        | (tr, acc, h::t, ns) -> (nextStates (tr, acc, t, ns) |> List.map (fun (ntr, na, nt, nns) -> (ntr, na, h::nt, nns)))
                        | _ -> raise <| new InvalidOperationException()

                    match current with
                    | totalRate, accumulated, position::others, network ->
                        seq {
                            let (rate, connections) = network |> Map.find position

                            let newPositions =
                                seq {
                                    yield! connections |> List.map(fun newPosition -> (totalRate, accumulated, newPosition::others, network))
                                    if rate > 0 then yield (totalRate + rate, accumulated, position::others, network |> Map.add position (0, connections))
                                }

                            match others with
                            | [] -> yield! newPositions
                            | _ -> yield! newPositions |> Seq.map mergeSimultaneousMove |> Seq.fold (@) []
                        } |> Seq.toList
                    | _ -> raise <| new InvalidOperationException()

                let getAccumulated (_, a, _, _) = a

                let rec runStep count stack =
                    let next =
                        stack
                        |> List.map nextStates
                        |> List.fold (@) []
                        |> List.map (fun (tr, acc, st, ne) -> (tr, acc + tr, st, ne))
                        |> List.sortByDescending getAccumulated

                    match count, next |> List.length with
                    | 1, _ -> next
                    | _, c when c > cull -> next |> List.take cull |> runStep (count - 1)
                    | _, _ -> next |> runStep (count - 1)

                runStep (count - 1) [ (0, 0, actorPositions, network) ] |> Seq.map getAccumulated |> Seq.max

            (run 30 [ "AA" ], run 26 [ "AA"; "AA" ])