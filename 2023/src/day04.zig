const std = @import("std");
const Allocator = std.mem.Allocator;

const util = @import("util.zig");
const gpa = util.gpa;
const print = std.debug.print;
const parseInt = std.fmt.parseInt;

const data = @embedFile("data/day04.txt");

pub fn main() !void {
    var part1: usize = 0;
    var part2: usize = 0;

    var lineCount: usize = 0;

    var lineIterator = std.mem.tokenize(u8, data, "\r\n");
    while (lineIterator.next()) |_|
        lineCount += 1;

    const copies = gpa.alloc(usize, lineCount) catch unreachable;
    for (0..lineCount) |i|
        copies[i] = 1;

    var lineIndex: usize = 0;
    lineIterator = std.mem.tokenize(u8, data, "\r\n");
    while (lineIterator.next()) |line| {
        var winningNumbers = std.AutoHashMap(usize, usize).init(gpa);

        var index: isize = 0;

        var it2 = std.mem.tokenizeAny(u8, line, ":|");
        while (it2.next()) |part| {
            if (index == 1) {
                var partIterator = std.mem.tokenize(u8, part, " ");
                while (partIterator.next()) |number| {
                    const n = parseInt(usize, number, 10) catch unreachable;
                    winningNumbers.put(n, 0) catch unreachable;
                }
            }

            if (index == 2) {
                var score: usize = 0;
                var hits: usize = 0;

                var partIterator = std.mem.tokenize(u8, part, " ");
                while (partIterator.next()) |number| {
                    const n = parseInt(usize, number, 10) catch unreachable;
                    if (winningNumbers.contains(n)) {
                        if (score == 0)
                            score = 1
                        else
                            score = score * 2;

                        hits += 1;
                    }
                }
                part1 += score;

                for (lineIndex + 1..lineIndex + 1 + hits) |i|
                    copies[i] += copies[lineIndex];
            }

            index += 1;
        }
        lineIndex += 1;
    }

    for (0..lineCount) |i|
        part2 += copies[i];

    print("Part 1: {d}\n", .{part1});
    print("Part 2: {d}\n", .{part2});
}
