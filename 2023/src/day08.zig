const std = @import("std");
const List = std.ArrayList;
const Map = std.AutoHashMap;
const print = std.debug.print;

const util = @import("util.zig");
const gpa = util.gpa;

const data = @embedFile("data/day08.txt");

const left = false;
const right = true;

const Node = struct {
    left: usize,
    right: usize
};

fn toUsize(s: []const u8) usize {
    var result: usize = 0;
    for (s) |c|
        result = result * 256 + c;
    return result;
}

var instructions = List(bool).init(gpa);
var nodeMap = Map(usize, Node).init(gpa);

fn getNextNode(current: usize, count: usize) usize {
    const node = nodeMap.get(current) orelse unreachable;
    const direction = instructions.items[count % instructions.items.len];
    if (direction == left)
        return node.left
    else
        return node.right;
}

pub fn main() !void {
    var first = true;

    defer instructions.deinit();
    defer nodeMap.deinit();

    var parallel = List(usize).init(gpa);
    defer parallel.deinit();

    var cycleLengths = List(usize).init(gpa);
    defer cycleLengths.deinit();

    var lineIterator = std.mem.tokenize(u8, data, "\r\n");
    while (lineIterator.next()) |line| {
        if (first) {
            for (line) |c| {
                instructions.append(switch(c) {
                    'L' => left,
                    'R' => right,
                    else => unreachable
                }) catch unreachable;
            }
            first = false;
            continue;
        }

        var parts = [_]usize{ 0, 0, 0 };
        var index: usize = 0;

        var tokenIterator = std.mem.tokenize(u8, line, " =(,)");
        while (tokenIterator.next()) |token| {
            parts[index] = toUsize(token);
            index += 1;
        }

        if (parts[0] % 256 == 'A')
            parallel.append(parts[0]) catch unreachable;

        nodeMap.put(parts[0], Node { .left = parts[1], .right = parts[2]}) catch unreachable;
    }

    var current: usize = toUsize("AAA");
    const goal: usize = toUsize("ZZZ");
    var part1: usize = 0;
    while (current != goal) {
        current = getNextNode(current, part1);
        part1 += 1;
    }
    print("Part 1: {d}\n", .{part1});

    for (0..parallel.items.len) |i| {
        var count: usize = 0;
        var position = parallel.items[i];
        while (cycleLengths.items.len <= i) {
            position = getNextNode(position, count);
            count += 1;

            if (position % 256 == 'Z')
                cycleLengths.append(count) catch unreachable;
        }
    }

    var factors: usize = 1;

    for (2..cycleLengths.items[0] / 2) |i| {
        var ok = true;
        for (cycleLengths.items) |l| {
            if (l % i == 0)
                continue;
            ok = false;
            break;
        }
        if (ok)
            factors *= i;
    }

    var part2 : u128 = 1;
    for (cycleLengths.items) |l|
        part2 *= l / factors;

    part2 *= factors;

    print("Part 2: {d}\n", .{part2});
}
