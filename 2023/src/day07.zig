const std = @import("std");
const List = std.ArrayList;

const util = @import("util.zig");
const gpa = util.gpa;

const data = @embedFile("data/day07.txt");
const print = std.debug.print;

const five_of_a_kind = 6;
const four_of_a_kind = 5;
const full_house = 4;
const three_of_a_kind = 3;
const two_pairs = 2;
const one_pair = 1;

const Match = struct { jokerCount: usize, counts: usize, handType: usize };

const matches = [_]Match{
    Match{ .jokerCount = 5, .counts = 5, .handType = five_of_a_kind },
    Match{ .jokerCount = 4, .counts = 14, .handType = five_of_a_kind },
    Match{ .jokerCount = 3, .counts = 23, .handType = five_of_a_kind },
    Match{ .jokerCount = 3, .counts = 113, .handType = four_of_a_kind },
    Match{ .jokerCount = 2, .counts = 23, .handType = five_of_a_kind },
    Match{ .jokerCount = 2, .counts = 122, .handType = four_of_a_kind },
    Match{ .jokerCount = 2, .counts = 1112, .handType = three_of_a_kind },
    Match{ .jokerCount = 1, .counts = 14, .handType = five_of_a_kind },
    Match{ .jokerCount = 1, .counts = 113, .handType = four_of_a_kind },
    Match{ .jokerCount = 1, .counts = 122, .handType = full_house },
    Match{ .jokerCount = 1, .counts = 1112, .handType = three_of_a_kind },
    Match{ .jokerCount = 1, .counts = 11111, .handType = one_pair },
    Match{ .jokerCount = 0, .counts = 5, .handType = five_of_a_kind },
    Match{ .jokerCount = 0, .counts = 14, .handType = four_of_a_kind },
    Match{ .jokerCount = 0, .counts = 23, .handType = full_house },
    Match{ .jokerCount = 0, .counts = 113, .handType = three_of_a_kind },
    Match{ .jokerCount = 0, .counts = 122, .handType = two_pairs },
    Match{ .jokerCount = 0, .counts = 1112, .handType = one_pair }
};

const Hand = struct {
    cards: [5]u8,
    bid: usize,
    score: usize,

    pub fn calculateScore(cards: []u8) usize {
        var counts = [15]usize{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        var maxCount: usize = 0;
        for (cards) |card| {
            counts[card] += 1;
            maxCount = @max(maxCount, counts[card]);
        }

        const jokerCount = counts[1];

        std.sort.heap(usize, &counts, {}, std.sort.desc(usize));

        var handType: usize = 0;

        for (matches) |match| {
            if (jokerCount != match.jokerCount)
                continue;

            var ok: bool = true;
            var matchCounts = match.counts;
            for (counts) |c| {
                if (matchCounts == 0)
                    break;

                if (c != matchCounts % 10) {
                    ok = false;
                    break;
                }

                matchCounts /= 10;
            }

            if (ok) {
                handType = match.handType;
                break;
            }
        }

        return handType * 10000000000 +
            @as(usize, @intCast(cards[0])) * 100000000 +
            @as(usize, @intCast(cards[1])) * 1000000 +
            @as(usize, @intCast(cards[2])) * 10000 +
            @as(usize, @intCast(cards[3])) * 100 +
            @as(usize, @intCast(cards[4]));
    }

    pub fn parse(line: []const u8, jokerValue: u8) Hand {
        var cards: [5]u8 = [_]u8{ 0, 0, 0, 0, 0 };
        var bid: usize = 0;

        var index: usize = 0;
        var it = std.mem.tokenize(u8, line, " ");
        while (it.next()) |token| {
            if (index == 0) {
                for (0..5) |i| {
                    cards[i] = switch (token[i]) {
                        'T' => 10,
                        'J' => jokerValue,
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
    const x = hands.toOwnedSlice() catch unreachable;
    std.sort.heap(Hand, x, {}, Hand.compare);

    var result: usize = 0;
    var index: usize = 0;

    for (x) |item| {
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
