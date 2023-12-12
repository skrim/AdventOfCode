const std = @import("std");
const List = std.ArrayList;

const util = @import("util.zig");
const gpa = util.gpa;

const data = @embedFile("data/day09.txt");
const print = std.debug.print;

fn extrapolateForward(comptime T: type, row : List(T)) T {
    var nextRow = List(T).init(gpa);
    defer nextRow.deinit();

    var allzeroes: bool = true;

    for (1..row.items.len) |i| {
        const d = row.items[i] - row.items[i - 1];
        if (d != 0)
            allzeroes = false;

        nextRow.append(d) catch unreachable;
    }
    if (allzeroes)
        return row.items[row.items.len - 1];

    const a = extrapolateForward(T, nextRow);
    const b = row.items[row.items.len - 1];
    return a + b;
}

fn extrapolateBackward(comptime T: type, row : List(T)) T {
    var nextRow = List(T).init(gpa);
    defer nextRow.deinit();

    var allzeroes: bool = true;

    for (1..row.items.len) |di| {
        const i = row.items.len - di;

        const d = row.items[i] - row.items[i - 1];
        if (d != 0)
            allzeroes = false;

        nextRow.insert(0, d) catch unreachable;
    }
    if (allzeroes)
        return row.items[0];

    const a = extrapolateBackward(T, nextRow);
    const b = row.items[0];
    return b - a;
}

pub fn main() !void {
    var row = List(isize).init(gpa);
    defer row.deinit();

    var part1 : isize = 0;
    var part2 : isize = 0;

    var lineIterator = std.mem.tokenize(u8, data, "\r\n");
    while (lineIterator.next()) |line| {
        var tokenIterator = std.mem.tokenize(u8, line, " =(,)");

        while (tokenIterator.next()) |token| {
            const v = std.fmt.parseInt(isize, token, 10) catch unreachable;
            row.append(v) catch unreachable;
        }

        part1 += extrapolateForward(isize, row);
        part2 += extrapolateBackward(isize, row);

        row.clearAndFree();
    }

    print("Part 1: {}\n", .{part1});
    print("Part 2: {}\n", .{part2});
}
