-module(program).
-export([start/0]).

flatten(X) -> lists:reverse(flatten(X,[])).

flatten([],Acc) -> Acc;
flatten([H|T],Acc) when is_list(H) -> flatten(T, flatten(H,Acc));
flatten([H|T],Acc) -> flatten(T,[H|Acc]).

loadData() ->
    {_, Data} = file:read_file("input.txt"),
    FilteredLines = [ string:trim(X, both, "\r") || X <- string:lexemes(erlang:binary_to_list(Data), "\n"), string:trim(X, both, "\r") =/= "" ],

    CalledValues = lists:map( fun(X) -> {Int, _} = string:to_integer(X), Int end, string:lexemes(lists:nth(1, FilteredLines), ",") ),

    BoardValues = flatten(
        [ lists:map( fun(X) -> {Int, _} = string:to_integer(X), Int end, N ) || N <-
            [ string:lexemes(X, " ") || X <- lists:nthtail(1, FilteredLines) ]
        ] ),

    % { Numbers, Hits, CompletedAt }
    Boards = [ { lists:sublist(BoardValues, 1 + X * 25, 25), 0, 9999 } || X <- lists:seq(0, trunc(length(BoardValues) / 25) - 1) ],

    { CalledValues, Boards }.

generateHitMasks() ->
    Horizontals = [ lists:seq(X * 5, X * 5 + 4) || X <- lists:seq(0, 4) ],
    Verticals = [ lists:seq(X, X + 24, 5) || X <- lists:seq(0, 4) ],
    %Diagonals = [ lists:seq(0, 24, 6), lists:seq(4, 20, 4) ],

    [ lists:sum([ 1 bsl Y || Y <- X ]) || X <- Horizontals ++ Verticals ].

getCompletion(Board, Iteration) ->
    { Numbers, Hits, CompletedAt } = Board,
    FoundHits = [ X || X <- generateHitMasks(), Hits band X =:= X ],
    if
        CompletedAt < Iteration -> Board;
        length(FoundHits) > 0 ->
            io:format("Completed at iteration ~p: ~p~n", [ Iteration, Numbers ]),
            { Numbers, Hits, Iteration };
        true -> Board
    end.

addHitInfo(Board, CurrentValue) ->
    { Numbers, Hits, CompletedAt } = Board,
    {
        Numbers,
        case CompletedAt of
            9999 -> Hits bor lists:sum( [ 1 bsl Pos || { Pos, V } <- lists:zip(lists:seq(0, 24), Numbers ), V =:= CurrentValue ]);
            _ -> Hits
        end,
        CompletedAt
    } .

hit(Boards, CalledValues, Iteration) ->
    case length(CalledValues) of
        0 ->
            Boards;
        _ ->
            CurrentValue = lists:nth(1, CalledValues),
            io:format("Calling ~p, Iteration ~p ~n", [ CurrentValue, Iteration ]),
            CompletionAdded = [ getCompletion(B, Iteration) || B <-
                [ addHitInfo(B, CurrentValue) || B <- Boards ]
            ],

            hit(CompletionAdded, lists:nthtail(1, CalledValues), Iteration + 1)
    end.

printResult(CalledValues, Board, Message) ->
    { Numbers, Hits, CompletedAt } = Board,
    CalledAtCompletion = lists:nth(CompletedAt + 1, CalledValues),
    Checksum = lists:sum([ V || { Pos, V } <- lists:zip(lists:seq(0,24), Numbers), (1 bsl Pos) band Hits =:= 0 ]) * CalledAtCompletion,

    io:format("~p: ~p~n", [Message, Checksum]).

start() ->
    { CalledValues, Boards } = loadData(),

    Result = lists:keysort(3, hit(Boards, CalledValues, 0)),

    printResult(CalledValues, lists:nth(1, Result), "Step 1"),
    printResult(CalledValues, lists:nth(1, lists:reverse(Result)), "Step 2"),

    Result.
