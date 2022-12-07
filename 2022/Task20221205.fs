namespace Skrim.AdventOfCode

type Task20221205 () =
    interface IAdventOfCodeTask<string> with
        override x.Solve(input) =
            let stackInfo =
                input
                |> Seq.takeWhile(fun f -> f <> "")
                |> Seq.rev

            let stackCount = (stackInfo |> Seq.head).Split(' ') |> Array.rev |> Array.head |> int32

            let initialStacks = (Array.create stackCount List.empty<char>) |> List.ofArray

            let stacks =
                stackInfo
                |> Seq.tail
                |> Seq.fold(fun state value ->
                    state
                    |> List.mapi(fun index stack ->
                        let pos = index * 4 + 1
                        if String.length value >= pos + 1 && value[pos] <> ' ' then
                            value[pos] :: stack
                        else
                            stack
                    )
                ) initialStacks

            let commands =
                input
                |> Seq.skipWhile(fun f -> not (f.StartsWith("m")))
                |> Seq.map(fun f ->
                    let t = f.Split(' ')
                    (t[1] |> int32, (t[3] |> int32) - 1, (t[5] |> int32) - 1)
                )

            let moveItems (stacks : list<list<char>>) source target count =
                let items = stacks[source] |> List.take count

                stacks
                |> List.mapi(fun index stack ->
                    match index with
                    | i when i = source -> stack |> List.skip count
                    | i when i = target -> items @ stack
                    | _ -> stack
                )

            let pickOneByOne stacks (count, source, target) =
                {1..count}
                |> Seq.fold(fun state _ -> moveItems state source target 1) stacks

            let pickMany stacks (count, source, target) =
                moveItems stacks source target count

            let result picker =
                commands
                |> Seq.fold picker stacks
                |> Seq.map List.head
                |> Seq.fold(fun state value -> state + (value |> string) ) ""

            (result pickOneByOne, result pickMany)
