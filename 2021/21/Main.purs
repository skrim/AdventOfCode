module Main ( main ) where

import Data.Traversable (sum)
import Prelude

import Data.Array (filter)
import Data.Int53 (Int53, toString, fromInt)
import Effect (Effect)
import Effect.Console (log)

-- Data.Int53 here: https://github.com/rgrempel/purescript-int-53/blob/master/src/Data/Int53.purs

type Player = { position :: Int, score :: Int, wins :: Int53 }
type GameState = { prev :: Player,  next :: Player, die :: Int, rollCount :: Int, multiplier :: Int53 }
type Turn = { die :: Int, delta :: Int }

getNewPos :: Int -> Int -> Int
getNewPos current delta | current + delta > 10 = getNewPos (current - 10) delta
                        | otherwise = current + delta

-- part 1

nextDie :: Int -> Int
nextDie die | die == 100 = 1
            | otherwise = die + 1

move :: Turn -> Int -> Turn
move turn count | count == 0 = turn
                | otherwise = move { die: nextDie turn.die, delta: turn.delta + nextDie turn.die } (count - 1)

part1Completed :: GameState -> Boolean
part1Completed state = state.prev.score >= 1000 || state.next.score >= 1000

next :: GameState -> GameState
next state = do
  let turn = move { die: state.die, delta: 0 } 3
  let newPos = getNewPos state.next.position turn.delta
  { die: turn.die,
    next: state.prev,
    prev: {
      position: newPos,
      score: state.next.score + newPos,
      wins: fromInt 0
    },
    rollCount: state.rollCount + 3,
    multiplier: fromInt 1
  }

final :: GameState -> GameState
final state | part1Completed state = state
            | otherwise = final (next state)

part1Result :: GameState -> Int
part1Result state = state.rollCount * (min state.next.score state.prev.score)

-- part 2

completionScore :: Int
completionScore = 21

winScore :: Int -> Int
winScore score | score >= completionScore = 1
               | otherwise = 0

part2Completed :: GameState -> Boolean
part2Completed state = state.prev.score >= completionScore || state.next.score >= completionScore

part2NotCompleted :: GameState -> Boolean
part2NotCompleted state = not (part2Completed state)

next2 :: RollAggregate -> GameState -> GameState
next2 roll state = do
  let newPos = getNewPos state.next.position roll.result
  let newScore = state.next.score + newPos
  {
    die: 0,
    next: state.prev,
    prev: {
      position: newPos,
      score: newScore,
      wins: fromInt $ winScore newScore
    },
    rollCount: 0,
    multiplier: state.multiplier * fromInt roll.count
  }

type Result = { wins1 :: Int53, wins2 :: Int53 }

type RollAggregate = { result :: Int, count :: Int }

rollResults :: Array RollAggregate
rollResults = [
  { result: 3, count: 1 },
  { result: 4, count: 3 },
  { result: 5, count: 6 },
  { result: 6, count: 7 },
  { result: 7, count: 6 },
  { result: 8, count: 3 },
  { result: 9, count: 1 }
]

iterate :: GameState -> Result
iterate state = do
  let nextStates = map (\result -> next2 result state) rollResults
  let complete = filter part2Completed nextStates

  let iterated = map (\st -> iterate st) $ filter part2NotCompleted nextStates

  let nextWins = sum $ map (\st -> st.multiplier * st.next.wins) complete

  {
    wins1: (sum $ map (\result -> result.wins2) iterated) + nextWins,
    wins2: (sum $ map (\result -> result.wins1) iterated) + (sum $ map (\st -> st.multiplier * st.prev.wins) complete)
  }

-- setup

initialState :: GameState
initialState =  { die: 100, rollCount: 0, multiplier: fromInt 1,
                  next: { position: 7, score: 0, wins: fromInt 0 },
                  prev: { position: 1, score: 0, wins: fromInt 0 }
                }

main :: Effect Unit
main = do
  log $ "Part 1: " <> (show $ part1Result $ final initialState)

  let result2 = iterate initialState
  log $ "Part 2: " <> (toString $ (max result2.wins1 result2.wins2))
