#!/usr/bin/swift

import Foundation

let lines = try String(contentsOfFile: "input.txt").split(separator: "\n")

var rules = [String: String]()

for line in lines {
    let tokens = line.split(separator: " ")
    if (tokens.count == 3) {
        rules[ String(tokens[0]) ] = String(tokens[2]);
    }
}

var initialization = String(lines[0])
var current = [String: Int]()

for pos in 0...initialization.count - 2 {
    let segment = String(initialization.prefix(pos + 2).suffix(2))
    current[segment] = (current[segment] ?? 0) + 1;
}

for i in 0...39 {
    var next = [String: Int]()
    var characterCounts = [String: Int]()

    for (key, value) in current {
        let r = rules[key]!;

        let f = String(key.prefix(1))
        let l = String(key.suffix(1))

        let first = f + r
        next[first] = (next[first] ?? 0) + value;

        let second = r + l
        next[second] = (next[second] ?? 0) + value;

        characterCounts[f] = (characterCounts[f] ?? 0) + value
        characterCounts[r] = (characterCounts[r] ?? 0) + value
    }
    let last = String(initialization.suffix(1))
    characterCounts[last] = (characterCounts[last] ?? 0) + 1

    if (i == 9 || i == 39) {
        print (characterCounts.values.max()! - characterCounts.values.min()!)
    }
    current = next
}
