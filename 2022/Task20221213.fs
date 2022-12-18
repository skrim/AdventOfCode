namespace Skrim.AdventOfCode

open System
open System.Text.RegularExpressions

type Node = { items : Node list; value : int32 }

type Task20221213 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =

            let emptyNode = { items = List.empty; value = -1 }
            let itemNode n = { emptyNode with items = n }
            let valueNode v = { emptyNode with value = v }

            let toNode data =
                let tokenize s = Regex("(\\[|\\d+|\\])").Matches(s) |> Seq.map(fun f -> f.Value) |> Seq.toList

                let rec toNodeStep data (current:Node) =
                    match data with
                    | h :: t when h = "[" ->
                        let nested = t |> List.takeWhile(fun v -> v <> "]")
                        let remaining = t |> List.skip (nested |> List.length)
                        toNodeStep remaining (itemNode ((toNodeStep nested emptyNode) :: current.items))
                    | h :: t when h = "]" -> toNodeStep t current
                    | h :: t -> toNodeStep t (itemNode ((valueNode (h |> int32)) :: current.items ))
                    | [] -> itemNode (List.rev current.items)

                toNodeStep (data |> tokenize |> List.tail) emptyNode

            let inputPairs =
                input |>
                Seq.fold(fun (list, prev) value ->
                    match value with
                    | v when v = "" -> (list, prev)
                    | v when prev = "" -> (list, v)
                    | v -> ((toNode prev, toNode v) :: list, "")
                ) (List.empty, "")
                |> fst
                |> List.rev

            let inputWithDividers =
                input
                |> Seq.filter (fun v -> v <> "")
                |> Seq.append [ "[2]"; "[6]" ]
                |> Seq.map toNode
                |> Seq.toList
                |> List.rev

            let compare a b =
                let rec compareStep a b =
                    match a.value, b.value, a.items, b.items with
                    | av, bv, _, _ when av >= 0 && bv >= 0 -> Math.Sign(av - bv)
                    | av, bv, _, _ when av < 0 && bv > 0 -> compareStep a (itemNode [ valueNode bv ])
                    | av, bv, _, _ when av > 0 && bv < 0 -> compareStep (itemNode [ valueNode av ]) b
                    | _, _, _::_, [] -> 1
                    | _, _, [], _::_ -> -1
                    | _, _, ah::at, bh::bt when ah.value >= 0 && ah.value = bh.value -> compareStep (itemNode at) (itemNode bt)
                    | _, _, ah::at, bh::bt ->
                        match compareStep ah bh with
                        | 0 -> compareStep (itemNode at) (itemNode bt)
                        | v -> v
                    | _, _, [], [] -> 0

                compareStep a b

            let part1 =
                inputPairs
                |> List.mapi (fun index (a, b) -> if compare a b = -1 then (index + 1) else 0 )
                |> List.sum

            let part2 =
                inputWithDividers
                |> List.sortWith compare
                |> List.fold(fun (index, product) value ->
                    match value.items with
                    | head::[] when head.value = 2 || head.value = 6 -> (index + 1, product * index)
                    | _ -> (index + 1, product)
                ) (1, 1)
                |> snd

            (part1, part2)