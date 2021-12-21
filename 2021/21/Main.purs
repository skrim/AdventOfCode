module Main ( main ) where

import Data.Traversable (sum)
import Math ((%))
import Prelude

import Data.Array (filter)
import Data.Int (fromNumber, toNumber)
import Data.Maybe (fromMaybe)
import Debug (trace)
import Effect (Effect)
import Effect.Console (log)

type Player = { position :: Int, score :: Int, wins :: Int }
type GameState = { prev :: Player,  next :: Player, die :: Int, rollCount :: Int, multiplier :: Int }
type Turn = { die :: Int, delta :: Int }

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
  let newPos = 1 + (fromMaybe 0 $ fromNumber $ ((toNumber (state.next.position + turn.delta) - 1.0) % 10.0))
  { die: turn.die,
    next: state.prev,
    prev: {
      position: newPos,
      score: state.next.score + newPos,
      wins: 0
    },
    rollCount: state.rollCount + 3,
    multiplier: 1
  }

final :: GameState -> (GameState -> Boolean) -> GameState
final state rule | rule state = state
                 | otherwise = final (next state) rule

part1Result :: GameState -> Int
part1Result state = state.rollCount * (min state.next.score state.prev.score)

-- part 2

completionScore :: Int
completionScore = 8

winScore :: Int -> Int
winScore score | score >= completionScore = 1
               | otherwise = 0

next2 :: Int -> GameState -> GameState
next2 delta state = do
  let newPos = 1 + (fromMaybe 0 $ fromNumber $ ((toNumber (state.next.position + delta) - 1.0) % 10.0))
  let newScore = state.next.score + newPos
  { die: 0,
    next: state.prev,
    prev: {
      position: newPos,
      score: newScore,
      wins: winScore newScore
    },
    rollCount: 0,
    multiplier: state.multiplier
  }

part2Completed :: GameState -> Boolean
part2Completed state = state.prev.score >= completionScore || state.next.score >= completionScore

part2NotCompleted :: GameState -> Boolean
part2NotCompleted state = not (part2Completed state)

type Result = { wins1 :: Int, wins2 :: Int }

resultSum :: Result -> Result -> Result
resultSum a b = { wins1: a.wins1 + b.wins1, wins2: a.wins2 + b.wins2 }

rollResults :: Array Int
rollResults = [ 3, 4, 5, 4, 5, 6, 5, 6, 7, 4, 5, 6, 5, 6, 7, 6, 7, 8, 5, 6, 7, 6, 7, 8, 7, 8, 9 ]

iterate :: GameState -> Result
iterate state = do
  let nextStates = map (\result -> next2 result state) rollResults
  let complete = filter part2Completed nextStates

  let wins = {
    wins1: (sum $ (map (\st -> st.multiplier * st.next.wins)) complete),
    wins2: (sum $ (map (\st -> st.multiplier * st.prev.wins)) complete)
  }

  let iterated = map (\st -> iterate st) $ filter part2NotCompleted nextStates

  let nestedWins = {
    wins1: sum (map (\result -> result.wins2) iterated),
    wins2: sum (map (\result -> result.wins1) iterated)
  }

  let result = resultSum wins nestedWins
  --trace (show result) \_ -> result
  result

-- setup

initialState :: GameState
initialState =  { die: 100, rollCount: 0, multiplier: 1,
                  next: { position: 7, score: 0, wins: 0 },
                  prev: { position: 1, score: 0, wins: 0 }
                }

main :: Effect Unit
main = do
  log $ "Part 1: " <> (show $ part1Result $ final initialState part1Completed)

  let result2 = iterate initialState
  log $ "Part 2: " <> (show $ (max result2.wins1 result2.wins2))
