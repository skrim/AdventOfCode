namespace Skrim.AdventOfCode
open System

type JobNode =
    | Value of int64
    | Calculation of (string*string*string)

type Task20221221 () =

    [<Literal>]
    let ROOT = "root"
    [<Literal>]
    let HUMN = "humn"

    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let evaluate v1 v2 = function
                | "+" -> v1 + v2
                | "-" -> v1 - v2
                | "*" -> v1 * v2
                | "/" -> v1 / v2
                | _ -> raise <| new InvalidOperationException()

            let evaluateBack v1 v2 swapped op =
                match op, swapped with
                | "/", true -> v1 * v2
                | "/", false -> v2 / v1
                | "-", true -> v1 + v2
                | "-", false -> v2 - v1
                | "*", _ -> v1 / v2
                | "+", _ -> v1 - v2
                | _ -> raise <| new InvalidOperationException()

            let parseLine (line:string) =
                match line[0..3], line.Split(" ") |> Array.toList with
                | name, _::p1::op::p2::[] -> name, Calculation(op, p1, p2)
                | name, _::value::[] -> name, Value(value |> int64)
                | _ -> raise <| new InvalidOperationException()

            let data = input |> Seq.map parseLine |> Map.ofSeq

            let resolveFor target tree =
                let rec resolveStep key stack =
                    let calculateStep (op, p1, p2) s0 =
                        let v1, s1, c1 = resolveStep p1 s0
                        let v2, s2, c2 = resolveStep p2 s1
                        let result = op |> evaluate v1 v2
                        match c1, c2 with
                        | false, false -> result, s2, false
                        | false, true -> result, (v1, op, false) :: s2, true
                        | true, false -> result, (v2, op, true) :: s2, true
                        | _ -> raise <| new InvalidOperationException()

                    match tree |> Map.find key with
                    | Value v -> (v, stack, key = HUMN)
                    | Calculation c -> calculateStep c stack

                let (result, stack, _) = resolveStep target []
                (result, stack)

            let (part1, stack) = data |> resolveFor ROOT
            let (rootValue, _, _) = stack |> List.head
            let part2 =
                stack
                |> List.tail
                |> List.fold (fun v1 (v2, op, swapped) -> op |> evaluateBack v1 v2 swapped) rootValue

            (part1, part2)