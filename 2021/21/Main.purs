module Main
  ( main )
  where

import Data.String.Pattern
import Data.Traversable
import Math hiding (min)
import Prelude

import Control.Monad.State (state)
import Data.Array (concat, difference, filter, head, length, null, tail)
import Data.Int (fromString, fromNumber, toNumber)
import Data.Maybe (Maybe)
import Data.Maybe (fromMaybe, maybe)
import Data.String (split)
import Data.String.NonEmpty (class MakeNonEmpty)
import Debug (trace)
import Effect (Effect)
import Effect.Class (liftEffect)
import Effect.Console (log, logShow)
import Node.Encoding (Encoding(..))
import Node.FS.Sync (readTextFile, readdir)

type Player = { position :: Int, score :: Int, wins :: Int }
type GameState = { prev :: Player,  next :: Player, die :: Int, rollCount :: Int }
type Turn = { die :: Int, delta :: Int }
type DieAggregate = { result :: Int, count :: Int }

aggregates :: Array DieAggregate
aggregates = [
  { result: 1, count: 1 },
  { result: 1, count: 1 },
  { result: 2, count: 1 },
  { result: 2, count: 1 }
]
{-
aggregates = [
  { result: 3, count: 1 },
  { result: 4, count: 3 },
  { result: 5, count: 6 },
  { result: 6, count: 7 },
  { result: 7, count: 6 },
  { result: 8, count: 3 },
  { result: 9, count: 1 }
]

aggregates = [
  { result: 3, count: 1 },
  { result: 4, count: 1 },
  { result: 5, count: 1 },
  { result: 4, count: 1 },
  { result: 5, count: 1 },
  { result: 6, count: 1 },
  { result: 5, count: 1 },
  { result: 6, count: 1 },
  { result: 7, count: 1 },
  { result: 4, count: 1 },
  { result: 5, count: 1 },
  { result: 6, count: 1 },
  { result: 5, count: 1 },
  { result: 6, count: 1 },
  { result: 7, count: 1 },
  { result: 6, count: 1 },
  { result: 7, count: 1 },
  { result: 8, count: 1 },
  { result: 5, count: 1 },
  { result: 6, count: 1 },
  { result: 7, count: 1 },
  { result: 6, count: 1 },
  { result: 7, count: 1 },
  { result: 8, count: 1 },
  { result: 7, count: 1 },
  { result: 8, count: 1 },
  { result: 9, count: 1 }
]
-}

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
    rollCount: state.rollCount + 3
  }

final :: GameState -> (GameState -> Boolean) -> GameState
final state rule | rule state = state
                 | otherwise = final (next state) rule

part1Result :: GameState -> Int
part1Result state = state.rollCount * (min state.next.score state.prev.score)

-- part 2

winScore :: Int -> Int
winScore score | score >= 21 = 1
               | otherwise = 0

next2 :: DieAggregate -> GameState -> GameState
next2 aggregate state = do
  let newPos = 1 + (fromMaybe 0 $ fromNumber $ ((toNumber (state.next.position + aggregate.result) - 1.0) % 10.0))
  let newScore = state.next.score + newPos
  { die: 0,
    next: state.prev,
    prev: {
      position: newPos,
      score: newScore,
      wins: aggregate.count * winScore newScore
    },
    rollCount: 0
  }

part2Completed :: GameState -> Boolean
part2Completed state = state.prev.score >= 21 || state.next.score >= 21

part2NotCompleted :: GameState -> Boolean
part2NotCompleted state = state.prev.score < 21 && state.next.score < 21

type Result = { wins1 :: Int, wins2 :: Int }

resultSum :: Result -> Result -> Result
resultSum a b = { wins1: a.wins1 + b.wins1, wins2: a.wins2 + b.wins2 }

iterate :: GameState -> Result
iterate state = do
  let nextStates = map (\aggregate -> next2 aggregate state) aggregates
  let complete = filter part2Completed nextStates

  let wins = {
    wins1: sum $ (map (\st -> st.next.wins)) complete,
    wins2: sum $ (map (\st -> st.prev.wins)) complete
  }

  let incomplete = filter part2NotCompleted nextStates
  let iterated = map (\st -> iterate st) incomplete

  let nestedWins = {
    wins1: sum (map (\result -> result.wins2) iterated),
    wins2: sum (map (\result -> result.wins1) iterated)
  }

  let result = resultSum wins nestedWins
  --trace (show result) \_ -> result
  result

--

initialState :: GameState
initialState =  { die: 100, rollCount: 0,
                  next: { position: 7, score: 0, wins: 0 },
                  prev: { position: 1, score: 0, wins: 0 }
                }

main :: Effect Unit
main = do
  log $ "Part 1: " <> (show $ part1Result $ final initialState part1Completed)
  part2

part2 :: Effect Unit
part2 = do
  log $ "Part 2: " <> (show $ iterate initialState)

