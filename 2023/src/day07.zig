const std = @import("std");
const List = std.ArrayList;

const util = @import("util.zig");
const gpa = util.gpa;

const data = @embedFile("data/day07.txt");
const print = std.debug.print;

const Match = struct {
    counts: usize,
    rank: usize,

    fn isMatch(self: Match, counts: []usize) bool {
        var matchCounts = self.counts;
        for (counts) |c| {
            if (matchCounts == 0)
                return true;

            if (c != matchCounts % 10)
                return false;

            matchCounts /= 10;
        }
        unreachable;
    }
};

const matches = [_]Match{
    .{ .counts = 5, .rank = 6 },
    .{ .counts = 14, .rank = 5 },
    .{ .counts = 23, .rank = 4 },
    .{ .counts = 113, .rank = 3 },
    .{ .counts = 122, .rank = 2 },
    .{ .counts = 1112, .rank = 1 },
    .{ .counts = 11111, .rank = 0 }
};

const Hand = struct {
    cards: [5]usize,
    bid: usize,
    score: usize,

    pub fn calculateScore(cards: []usize) usize {
        var counts = [15]usize{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        for (cards) |card|
            counts[card] += 1;

        const jokerCount = counts[1];
        counts[1] = 0;

        std.sort.heap(usize, &counts, {}, std.sort.desc(usize));
        counts[0] += jokerCount;

        for (matches) |match| {
            if (!match.isMatch(&counts))
                continue;

            var score: usize = match.rank;
            for (cards) |c|
                score = score * 15 + c;
            return score;
        }
        unreachable;
    }

    pub fn parse(line: []const u8, jValue: usize) Hand {
        var cards: [5]usize = [_]usize{ 0, 0, 0, 0, 0 };
        var bid: usize = 0;

        var index: usize = 0;
        var it = std.mem.tokenize(u8, line, " ");
        while (it.next()) |token| {
            if (index == 0) {
                for (0..5) |i| {
                    cards[i] = switch (token[i]) {
                        'T' => 10,
                        'J' => jValue,
                        'Q' => 12,
                        'K' => 13,
                        'A' => 14,
                        '2'...'9' => token[i] - '0',
                        else => unreachable,
                    };
                }
            }
            if (index == 1)
                bid = std.fmt.parseInt(usize, token, 10) catch unreachable;

            index += 1;
        }

        return Hand{ .cards = cards, .bid = bid, .score = calculateScore(&cards) };
    }

    pub fn compare(_: void, a: Hand, b: Hand) bool {
        return a.score < b.score;
    }
};

fn calculateWinnings(hands: *List(Hand)) usize {
    const slice = hands.toOwnedSlice() catch unreachable;
    std.sort.heap(Hand, slice, {}, Hand.compare);

    var result: usize = 0;
    var index: usize = 0;

    for (slice) |item| {
        index += 1;
        result += item.bid * index;
    }
    return result;
}

pub fn main() !void {
    var hands1 = List(Hand).init(gpa);
    defer hands1.deinit();

    var hands2 = List(Hand).init(gpa);
    defer hands2.deinit();

    var lineIterator = std.mem.tokenize(u8, data, "\r\n");
    while (lineIterator.next()) |line| {
        hands1.append(Hand.parse(line, 11)) catch unreachable;
        hands2.append(Hand.parse(line, 1)) catch unreachable;
    }
    print("Part 1: {d}\n", .{calculateWinnings(&hands1)});
    print("Part 2: {d}\n", .{calculateWinnings(&hands2)});
}
