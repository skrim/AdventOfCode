const std = @import("std");
const data = @embedFile("data/day01.txt");

fn getValues(tokens: []const []const u8, lines: []const u8) usize {
    var it = std.mem.split(u8, lines, "\n");

    var result: usize = 0;
    while (it.next()) |line| {
        var firstPos: usize = 999;
        var firstIndex: usize = 999;
        var lastPos: usize = 999;
        var lastIndex: usize = 999;

        for (0.., tokens) |index, token| {
            const first = std.mem.indexOfPos(u8, line, 0, token);
            if (first) |f| {
                if (f < firstPos or firstIndex == 999) {
                    firstPos = f;
                    firstIndex = index;
                }
            }

            if (line.len >= token.len) {
                const last = std.mem.lastIndexOfLinear(u8, line, token);
                if (last) |l| {
                    if (l > lastPos or lastIndex == 999) {
                        lastPos = l;
                        lastIndex = index;
                    }
                }
            }
        }
        result += (firstIndex % 10) * 10 + (lastIndex % 10);
    }
    return result;
}

pub fn main() !void {
    const stdout = std.io.getStdOut().writer();
    const numbersAndWords = [_][]const u8{ "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
    try stdout.print("Part 1: {}\n", .{getValues((numbersAndWords)[0..10], data)});
    try stdout.print("Part 2: {}\n", .{getValues((numbersAndWords)[0..], data)});
}
