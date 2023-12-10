const std = @import("std");
const Allocator = std.mem.Allocator;
const List = std.ArrayList;

const util = @import("util.zig");
const gpa = util.gpa;

const data = @embedFile("data/day05.txt");

const print = std.debug.print;

const Mapping = struct {
    from: isize,
    range: isize,
    delta: isize,

    pub fn getDelta(self: Mapping, value: isize) isize {
        if (value < self.from or value >= self.from + self.range)
            return 0;

        return self.delta;
    }

    pub fn mapRange(self: Mapping, range: Range) Range {
        if (range.start >= self.from + self.range or range.end < self.from)
            return Range{ .start = 0, .end = 0, .valid = false };

        return Range{ .start = @max(range.start, self.from) + self.delta, .end = @min(range.end, self.from + self.range) + self.delta, .valid = true };
    }
};

const Range = struct { start: isize, end: isize, valid: bool };

const Mapper = struct {
    mappings: List(Mapping),

    pub fn map(self: Mapper, value: isize) isize {
        var result = value;
        for (self.mappings.items) |mapping| {
            const delta = mapping.getDelta(result);
            if (delta == 0)
                continue;

            result += delta;
            break;
        }
        return result;
    }

    pub fn mapRanges(self: Mapper, ranges: List(Range)) List(Range) {
        var result = List(Range).init(gpa);

        for (ranges.items) |range| {
            for (self.mappings.items) |mapping| {
                const mappedRange = mapping.mapRange(range);
                if (!mappedRange.valid)
                    continue;
                result.append(mappedRange) catch unreachable;
            }
        }

        return result;
    }

    pub fn addMapping(self: *Mapper, from: isize, range: isize, delta: isize) void {
        if (self.mappings.items.len == 0)
            self.mappings = List(Mapping).init(gpa);

        const m = Mapping{ .from = from, .range = range, .delta = delta };
        self.mappings.append(m) catch unreachable;
    }
};

pub fn main() !void {
    var first: bool = true;
    var lineIterator = std.mem.tokenize(u8, data, "\r\n");
    var seeds = List(isize).init(gpa);
    defer seeds.deinit();

    var mappers = List(Mapper).init(gpa);
    defer mappers.deinit();

    while (lineIterator.next()) |line| {
        if (first) {
            var seedIterator = std.mem.tokenize(u8, line, "seeds: ");
            while (seedIterator.next()) |seed|
                seeds.append(std.fmt.parseInt(isize, seed, 10) catch unreachable) catch unreachable;

            first = false;
            continue;
        }

        if (std.mem.indexOf(u8, line, ":") orelse 0 > 0) {
            const newMapper = Mapper{ .mappings = undefined };
            mappers.append(newMapper) catch unreachable;
            continue;
        }

        var values = [3]isize{ 0, 0, 0 };
        var i: usize = 0;
        var mappingIterator = std.mem.tokenize(u8, line, " ");
        while (mappingIterator.next()) |v| {
            values[i] = std.fmt.parseInt(isize, v, 10) catch unreachable;
            i += 1;
        }

        var currentMapper = &mappers.items[mappers.items.len - 1];
        currentMapper.addMapping(values[1], values[2], values[0] - values[1]);
    }

    var part1: isize = std.math.maxInt(isize);

    for (seeds.items) |seed| {
        var result = seed;

        for (mappers.items) |mapper|
            result = mapper.map(result);

        part1 = @min(part1, result);
    }
    print("Part 1: {d}\n", .{part1});

    var ranges = List(Range).init(gpa);

    var i: usize = 0;
    while (i < seeds.items.len) : (i += 2)
        ranges.append(Range{ .start = seeds.items[i], .end = seeds.items[i] + seeds.items[i + 1] - 1, .valid = true }) catch unreachable;

    for (mappers.items) |mapper| {
        const newRanges = mapper.mapRanges(ranges);
        ranges.deinit();
        ranges = newRanges;
    }

    var part2: isize = std.math.maxInt(isize);
    for (ranges.items) |range|
        part2 = @min(part2, range.start);
    defer ranges.deinit();

    print("Part 2: {d}\n", .{part2});
}
