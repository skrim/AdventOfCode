const std = @import("std");
const List = std.ArrayList;

const print = std.debug.print;

const util = @import("util.zig");
const gpa = util.gpa;

const data = @embedFile("data/day11.txt");

const Galaxy: type = struct {
    x: usize,
    y: usize
};

var galaxies = List(Galaxy).init(gpa);
var emptyColumns = List(bool).init(gpa);
var emptyRows = List(bool).init(gpa);

fn calculateDistance(pos1: usize, pos2: usize, emptyItems: []bool, emptyDistance: usize) usize {
    var distance : usize = 0;
    for (@min(pos1, pos2) .. @max(pos1, pos2)) |pos| {
        if (emptyItems[pos])
            distance += emptyDistance
        else
            distance += 1;
    }
    return distance;
}

fn calculateDistances(emptyDistance: usize) usize {
    var distance: usize = 0;

    for (galaxies.items[0 .. galaxies.items.len - 1], 0 .. galaxies.items.len - 1) |g1, index| {
        for (galaxies.items[index + 1 .. galaxies.items.len]) |g2| {
            distance += calculateDistance(g1.x, g2.x, emptyColumns.items, emptyDistance);
            distance += calculateDistance(g1.y, g2.y, emptyRows.items, emptyDistance);
        }
    }
    return distance;
}

pub fn main() !void {
    var height: usize = 0;
    var lineIterator = std.mem.tokenize(u8, data, "\r\n");
    while (lineIterator.next()) |line| : (height += 1) {
        emptyRows.append(true) catch unreachable;

        for (line, 0..) |c, w| {
            if (height == 0)
                emptyColumns.append(true) catch unreachable;

            if (c == '#') {
                galaxies.append(Galaxy{.x = w, .y = height}) catch unreachable;
                emptyColumns.items[w] = false;
                emptyRows.items[height] = false;
            }
        }
    }

    print("Part 1: {}\n", .{calculateDistances(1)});
    print("Part 2: {}\n", .{calculateDistances(1000000)});
}
