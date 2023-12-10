const std = @import("std");
const Allocator = std.mem.Allocator;
const List = std.ArrayList;

const util = @import("util.zig");
const gpa = util.gpa;

const data = @embedFile("data/day02.txt");
const parseInt = std.fmt.parseInt;
const print = std.debug.print;

fn getColorIndex(color: []const u8) usize {
    if (std.mem.eql(u8, color, "red"))
        return 0;
    if (std.mem.eql(u8, color, "green"))
        return 1;
    if (std.mem.eql(u8, color, "blue"))
        return 2;
    unreachable;
}

const Match = struct {
    colors: [3]usize = [3]usize{ 0, 0, 0 },

    pub fn parse(line: []const u8) Match {
        var result = Match{};
        var index: usize = 0;
        var count: usize = 0;
        var it = std.mem.tokenizeAny(u8, line, ", ");
        while (it.next()) |part| {
            if (index % 2 == 0)
                count = parseInt(usize, part, 10) catch unreachable
            else
                result.colors[getColorIndex(part)] = count;
            index += 1;
        }
        return result;
    }
};

const Game = struct {
    id: usize,
    matches: std.ArrayList(Match) = std.ArrayList(Match).init(gpa),

    pub fn parseMatches(self: *Game, line: []const u8) !void {
        var it = std.mem.tokenizeAny(u8, line, ";");
        while (it.next()) |matchData|
            self.matches.append(Match.parse(matchData)) catch unreachable;
    }

    pub fn isPossible(self: *const Game) bool {
        for (self.matches.items) |match| {
            var totals = [3]usize{ 0, 0, 0 };
            for (0..3) |index|
                totals[index] += match.colors[index];

            if (totals[0] > 12 or totals[1] > 13 or totals[2] > 14)
                return false;
        }
        return true;
    }

    pub fn powerValue(self: *const Game) usize {
        var maxCounts = [3]usize{ 1, 1, 1 };
        for (self.matches.items) |match| {
            for (0..3) |index| {
                if (match.colors[index] > maxCounts[index])
                    maxCounts[index] = match.colors[index];
            }
        }

        return maxCounts[0] * maxCounts[1] * maxCounts[2];
    }

    pub fn parse(line: []const u8) Game {
        var parts = std.mem.tokenize(u8, line, ":");
        var game = Game{ .id = 0, .matches = std.ArrayList(Match).init(gpa) };

        var index: usize = 0;
        while (parts.next()) |part| {
            if (index == 0)
                game = Game{ .id = parseInt(usize, part[5..], 10) catch unreachable };
            if (index == 1)
                game.parseMatches(part) catch unreachable;
            index += 1;
        }

        return game;
    }
};

pub fn main() !void {
    var it = std.mem.tokenize(u8, data, "\r\n");
    var part1: usize = 0;
    var part2: usize = 0;
    while (it.next()) |line| {
        var game = Game.parse(line);

        if (game.isPossible())
            part1 += game.id;

        part2 += game.powerValue();
    }
    print("Part 1: {d}\n", .{part1});
    print("Part 2: {d}\n", .{part2});
}
