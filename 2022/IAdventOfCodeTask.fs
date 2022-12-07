namespace Skrim.AdventOfCode

type IAdventOfCodeTask = interface end

type IAdventOfCodeTask<'T> =
    inherit IAdventOfCodeTask
    abstract Solve : seq<string> -> 'T * 'T
