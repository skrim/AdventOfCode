namespace Skrim.AdventOfCode

type IAdventOfCodeTask = interface end

type IAdventOfCodeTask<'T1, 'T2> =
    inherit IAdventOfCodeTask
    abstract Solve : seq<string> -> 'T1 * 'T2

type IAdventOfCodeTask<'T> =
    inherit IAdventOfCodeTask<'T, 'T>
