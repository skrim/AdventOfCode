namespace Skrim.AdventOfCode

type Task20221220 () =
    interface IAdventOfCodeTask<int64> with
        override x.Solve(input) =
            let data = input |> Seq.map int64 |> Seq.toList

            let calculate data iterations =
                let initialValues = data |> List.mapi (fun i v -> (i, v))
                let length32 = initialValues.Length
                let length64 = initialValues.Length |> int64

                let move state order =
                    let index = state |> List.findIndex(fun (i, _) -> i = order)
                    let (_, delta) = state[index]
                    let newPos =
                        match ((int64 index) + delta) % (length64 - 1L) with
                        | 0L -> length64 - 1L
                        | np when np < 0 -> length64 - 1L + np
                        | np -> np

                    state |> List.removeAt index |> List.insertAt (newPos |> int32) (order, delta)

                let mixStep values = { 0 .. length32 - 1 } |> Seq.fold move values

                let mixed = { 1 .. iterations } |> Seq.fold (fun s _ -> mixStep s ) initialValues

                let final = mixed |> List.map snd
                let zeroIndex = final |> List.findIndex(fun v -> v = 0L)
                { 1000..1000..3000 } |> Seq.fold (fun s i -> s + final[(zeroIndex + i) % length32] ) 0L

            (calculate data 1, calculate (data |> List.map (fun v -> v * 811589153L)) 10)