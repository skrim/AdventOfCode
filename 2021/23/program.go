package main

import (
	"bufio"
	"os"
	"regexp"
)

var costPerType = []int{0, 1, 10, 100, 1000}

type WorldCost struct {
	World World
	Cost  int
}

type World struct {
	Height byte
	State1 int64
	State2 int64
}

var EmptyWorldCost = WorldCost{}

func MakeWorld(data string) World {
	result := World{Height: byte(len(data))/4 + 1}

	for i := byte(0); i < byte(len(data)); i++ {
		result = result.SetNode((i%4)*2+2, i/4+1, byte(data[i])-64)
	}
	return result
}

func GetBitIndex(x byte, y byte) byte {
	if y == 0 {
		return x * 3
	}
	return ((y-1)*4 + (x-2)/2) * 3
}

func (w World) GetNode(x byte, y byte) byte {
	if y == 0 {
		return (byte)((w.State1 >> GetBitIndex(x, y)) & 7)
	}
	return (byte)((w.State2 >> GetBitIndex(x, y)) & 7)
}

func ChangeState(x byte, y byte, s byte, worldState int64) int64 {
	return worldState & ^(7<<GetBitIndex(x, y)) | (int64(s) << GetBitIndex(x, y))
}

func (w World) SetNode(x byte, y byte, s byte) World {
	if y == 0 {
		return World{Height: w.Height, State1: ChangeState(x, y, s, w.State1), State2: w.State2}
	}
	return World{Height: w.Height, State1: w.State1, State2: ChangeState(x, y, s, w.State2)}
}

func (w World) Move(s byte, fromX byte, fromY byte, toX byte, toY byte) World {
	return w.SetNode(fromX, fromY, 0).SetNode(toX, toY, s)
}

func IsTargetColumn(x byte) bool {
	return x == 2 || x == 4 || x == 6 || x == 8
}

func IsNode(x byte, y byte) bool {
	return y == 0 || IsTargetColumn(x)
}

func (w World) IsHallwayFree(x1 byte, x2 byte) bool {
	if x2 < x1 {
		return w.IsHallwayFree(x2, x1)
	}

	for i := x1; i <= x2; i++ {
		if w.GetNode(i, 0) != 0 {
			return false
		}
	}
	return true
}

func Difference(a byte, b byte) int {
	if a < b {
		return int(b) - int(a)
	}
	return int(a) - int(b)
}

func (w World) MoveFromHallway(x byte, item byte, previousCost int) WorldCost {
	targetX := item * 2
	targetY := byte(0)
	okToEnter := true
	for i := byte(1); i < w.Height && okToEnter; i++ {
		st := w.GetNode(targetX, i)
		if st == 0 {
			targetY = i
		} else {
			okToEnter = st == item
		}
	}
	if !okToEnter {
		return EmptyWorldCost
	}

	nx := x - 1
	if targetX > x {
		nx = x + 1
	}

	if !w.IsHallwayFree(nx, targetX) {
		return EmptyWorldCost
	}

	return WorldCost{
		World: w.Move(item, x, 0, targetX, targetY),
		Cost:  previousCost + costPerType[item]*(int(targetY)+Difference(targetX, x))}
}

func (w World) MoveFromRoom(x byte, y byte, item byte, previousCost int) []WorldCost {
	var result []WorldCost

	if x == item*2 {
		sameBelow := true
		for i := y + 1; i < w.Height && sameBelow; i++ {
			sameBelow = w.GetNode(x, i) == item
		}
		if sameBelow {
			return result // completed: in target room and no other types below
		}
	}

	emptyAbove := true
	for i := y - 1; i > 0 && emptyAbove; i-- {
		emptyAbove = w.GetNode(x, i) == 0
	}
	if !emptyAbove {
		return result
	}

	for i := byte(8); i > byte(0); i-- {
		tx := x + i
		if tx < 11 && !IsTargetColumn(tx) && w.IsHallwayFree(x, tx) {
			result = append(result, WorldCost{World: w.Move(item, x, y, tx, 0), Cost: previousCost + costPerType[item]*(int(y)+Difference(tx, x))})
		}
		tx = x - i
		if x >= i && !IsTargetColumn(tx) && w.IsHallwayFree(x, tx) {
			result = append(result, WorldCost{World: w.Move(item, x, y, tx, 0), Cost: previousCost + costPerType[item]*(int(y)+Difference(tx, x))})
		}
	}
	return result
}

func (w World) GetMoves(previousCost int) []WorldCost {
	var result []WorldCost
	for y := byte(0); y < w.Height; y++ {
		for x := byte(0); x < byte(11); x++ {
			if !IsNode(x, y) {
				continue
			}
			item := w.GetNode(x, y)
			if item == 0 {
				continue
			}

			if y == 0 {
				move := w.MoveFromHallway(x, item, previousCost)
				if move != EmptyWorldCost {
					result = append(result, move)
				}
			} else {
				result = append(result, w.MoveFromRoom(x, y, item, previousCost)...)
			}
		}
	}
	return result
}

func solve(initial World, target World) int {
	var stack []WorldCost
	stack = append(stack, WorldCost{World: initial, Cost: 0})
	var lowestCost int = 999999

	usedStates := make(map[World]int)

	for len(stack) > 0 {
		n := len(stack) - 1
		item := stack[n]
		stack = stack[:n]

		if item.World == target && item.Cost < lowestCost {
			lowestCost = item.Cost
		}

		prevCost := usedStates[item.World]
		if prevCost != 0 && prevCost < item.Cost {
			continue
		}
		usedStates[item.World] = item.Cost

		stack = append(stack, item.World.GetMoves(item.Cost)...)
	}
	return lowestCost
}

func main() {
	file, _ := os.Open("input.txt")
	defer file.Close()

	scanner := bufio.NewScanner(file)
	var rx, _ = regexp.Compile(`[ABCD]`)
	source := ""
	for scanner.Scan() {
		matched := rx.FindAllString(scanner.Text(), -1)
		for _, t := range matched {
			source += t
		}
	}
	world1 := MakeWorld(source)
	target1 := MakeWorld("ABCDABCD")
	println("Part 1:", solve(world1, target1))

	world2 := MakeWorld(source[0:4] + "DCBADBAC" + source[4:])
	target2 := MakeWorld("ABCDABCDABCDABCD")
	println("Part 2:", solve(world2, target2))
}
