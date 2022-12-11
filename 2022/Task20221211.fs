namespace Skrim.AdventOfCode

open System
open System.Text.RegularExpressions

type MonkeyState =
    {
        items : uint64 list;
        operation : uint64 -> uint64;
        divisor : uint64;
        successTarget : int;
        failTarget : int;
        inspections : uint64
    }

type Task20221211 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =

            let defaultMonkeyState = { items = List.empty; operation = id; divisor = 1UL; successTarget = -1; failTarget= -1; inspections = 0UL }

            let initialState =
                let (|Int32|_|) str = Some(int32 <| str)
                let (|UInt64List|_|) (str:string) = Some(str.Split(", ") |> Array.map uint64 |> Array.toList)
                let (|Operator|_|) str =
                    match str with
                    | "*" -> Some((*))
                    | "+" -> Some((+))
                    | _ -> raise <| new InvalidOperationException()
                let (|Parameter|_|) str =
                    match str with
                    | "old" -> Some(fun v -> v)
                    | p -> Some(fun _ -> p |> uint64)
                let (|ParseRegex|_|) regex str =
                    let m = Regex(regex).Match(str)
                    if m.Success then Some (List.tail [ for x in m.Groups -> x.Value ]) else None

                let parseRow (target:MonkeyState) row =
                    match row with
                    | ParseRegex "^Monkey \\d+:$" [] -> target
                    | ParseRegex "^\\s*Starting items: ((?:\\d+)(?:,\\s\\d+)*)$" [UInt64List values] ->
                        { target with items = values }
                    | ParseRegex "^\\s*Operation: new = (old|\\d+) (\\+|\\*) (old|\\d+)$" [Parameter p1; Operator op; Parameter p2] ->
                        { target with operation = fun v -> op (p1 v) (p2 v) }
                    | ParseRegex "^\\s*Test: divisible by (\\d+)$" [Int32 d] -> { target with divisor = uint64 <| d }
                    | ParseRegex "^\\s*If true: throw to monkey (\\d+)$" [Int32 m] -> { target with successTarget = m }
                    | ParseRegex "^\\s*If false: throw to monkey (\\d+)$" [Int32 m] -> { target with failTarget = m }
                    | _ -> raise <| new InvalidOperationException(sprintf "Cannot parse '%s'" row)

                input
                |> Seq.fold(fun state value ->
                    match state with
                    | head :: tail when value = "" -> defaultMonkeyState :: head :: tail
                    | head :: tail -> (parseRow head value) :: tail
                    | _ -> raise <| new InvalidOperationException()
                ) [ defaultMonkeyState ]
                |> List.rev

            let productOfDivisors = initialState |> List.map (fun m -> m.divisor) |> List.fold (*) 1UL

            let runStep currentState (divisor:uint64) =
                [ 0 .. List.length currentState - 1 ]
                |> List.fold (fun (newState:MonkeyState list) currentMonkeyIndex ->
                    let currentMonkey = newState[currentMonkeyIndex]

                        currentMonkey.items
                        |> List.fold (fun stateAfterItem item ->
                        let worryLevel = ((currentMonkey.operation item) / divisor) % productOfDivisors
                            let target = if worryLevel % currentMonkey.divisor = 0UL then currentMonkey.successTarget else currentMonkey.failTarget

                            stateAfterItem
                            |> List.mapi (fun targetIndex otherMonkey ->
                                match targetIndex with
                                | t when t = target -> { otherMonkey with items = worryLevel :: otherMonkey.items }
                                | t when t = currentMonkeyIndex -> { otherMonkey with items = List.empty; inspections = otherMonkey.inspections + 1UL }
                                | _ -> otherMonkey
                            )
                        ) newState
                ) currentState

            let finalState1 = { 1..20 } |> Seq.fold (fun s _ -> runStep s 3UL) initialState
            let finalState2 = { 1..10000 } |> Seq.fold (fun s _ -> runStep s 1UL) initialState

            let result state =
                state
                |> List.map (fun m -> m.inspections)
                |> List.sortByDescending id
                |> List.take 2
                |> List.fold (*) 1UL
                |> int64

            (result finalState1, result finalState2)