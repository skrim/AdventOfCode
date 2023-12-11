const std = @import("std");
const List = std.ArrayList;

const util = @import("util.zig");
const gpa = util.gpa;

const data = @embedFile("data/day06.txt");
const print = std.debug.print;

fn calculateRange(time: f64, distance: f64) usize {
    const sq = std.math.sqrt(time * time - 4.0 * distance);
    const x1 = (time - sq) / 2.0;
    const x2 = (time + sq) / 2.0;

    var x1ceil = std.math.ceil(x1);
    var x2floor = std.math.floor(x2);

    if (x1ceil == x1)
        x1ceil += 1.0;

    if (x2floor == x2)
        x2floor -= 1.0;

    return @as(usize, @intFromFloat((x2floor - x1ceil + 1)));
}

pub fn main() !void {
    var times = List(f64).init(gpa);
    defer times.deinit();

    var distances = List(f64).init(gpa);
    defer distances.deinit();

    var p2time: f64 = 0.0;
    var p2distance: f64 = 0.0;

    var firstLine = true;
    var lineIterator = std.mem.tokenize(u8, data, "\r\n");
    while (lineIterator.next()) |line| {
        var valueIterator = std.mem.tokenize(u8, line, "Time: Distance:");

        var combined: isize = 0;

        while (valueIterator.next()) |value| {
            const v = std.fmt.parseFloat(f64, value) catch unreachable;
            if (firstLine)
                times.append(v) catch unreachable
            else
                distances.append(v) catch unreachable;

            for (value) |c|
                combined = combined * 10 + c - '0';
        }

        if (firstLine)
            p2time = @as(f64, @floatFromInt(combined))
        else
            p2distance = @as(f64, @floatFromInt(combined));

        firstLine = false;
    }

    var part1: usize = 1;
    for (times.items, distances.items) |time, distance|
        part1 *= calculateRange(time, distance);

    const part2 = calculateRange(p2time, p2distance);

    print("Part 1: {d}\n", .{part1});
    print("Part 2: {d}\n", .{part2});
}
