const std = @import("std");
const Allocator = std.mem.Allocator;

const util = @import("util.zig");
const gpa = util.gpa;
const print = std.debug.print;

const data = @embedFile("data/day03.txt");

const Area = struct {
    data: []u8,
    gearValues: []isize,
    width: isize,
    height: isize,

    fn get(self: Area, x: isize, y: isize) u8 {
        const p = @as(usize, @intCast(y * self.width + x));
        return self.data[p];
    }

    pub fn isGear(self: Area, x: isize, y: isize) bool {
        return self.isSymbol(x, y) and self.get(x, y) == '*';
    }

    pub fn isSymbol(self: Area, x: isize, y: isize) bool {
        if (x < 0 or x >= self.width or y < 0 or y >= self.height)
            return false;

        return self.get(x, y) != '.' and !self.isDigit(x, y);
    }

    pub fn isDigit(self: Area, x: isize, y: isize) bool {
        if (x < 0 or x >= self.width or y < 0 or y >= self.height)
            return false;

        const c = self.get(x, y);
        return c >= '0' and c <= '9';
    }

    pub fn isPartNumber(self: Area, x1: isize, x2: isize, y: isize) bool {
        if (self.isSymbol(x1 - 1, y) or self.isSymbol(x2 + 1, y))
            return true;

        var x = x1 - 1;
        while (x <= x2 + 1) : (x += 1) {
            if (self.isSymbol(x, y - 1) or self.isSymbol(x, y + 1))
                return true;
        }

        return false;
    }

    pub fn findGear(self: Area, x1: isize, x2: isize, y: isize) isize {
        if (self.isGear(x1 - 1, y))
            return (x1 - 1) + y * self.width;

        if (self.isGear(x2 + 1, y))
            return (x2 + 1) + y * self.width;

        var x = x1 - 1;
        while (x <= x2 + 1) : (x += 1) {
            if (self.isGear(x, y - 1))
                return x + (y - 1) * self.width;

            if (self.isGear(x, y + 1))
                return x + (y + 1) * self.width;
        }
        return -1;
    }

    pub fn load(lines: []const u8) Area {
        var width: isize = 0;
        var height: isize = 0;

        var it = std.mem.tokenize(u8, lines, "\r\n");
        while (it.next()) |line| {
            width = @as(isize, @intCast(line.len));
            height += 1;
        }

        const area = gpa.alloc(u8, @as(usize, @intCast(width * height))) catch unreachable;
        const gearValues = gpa.alloc(isize, @as(usize, @intCast(width * height))) catch unreachable;
        const result = Area{ .data = area, .gearValues = gearValues, .width = width, .height = height };
        it = std.mem.tokenize(u8, lines, "\r\n");
        var pos: usize = 0;
        while (it.next()) |line| {
            for (line) |c| {
                gearValues[pos] = 0;
                area[pos] = c;
                pos += 1;
            }
        }
        return result;
    }

    pub fn finalizeNumber(self: Area, x1: isize, x2: isize, y: isize, activeNumber: isize) bool {
        const gearPos = self.findGear(x1, x2, y);
        if (gearPos >= 0) {
            const p = @as(usize, @intCast(gearPos));

            if (self.gearValues[p] == 0)
                self.gearValues[p] = -activeNumber
            else
                self.gearValues[p] = self.gearValues[p] * activeNumber * -1;
        }

        return self.isPartNumber(x1, x2, y);
    }
};

pub fn main() !void {
    var part1: isize = 0;

    const area = Area.load(data);

    var activeNumber: isize = 0;
    var numberStart: isize = 0;

    var y: isize = 0;
    while (y < area.height) : (y += 1) {
        var x: isize = 0;
        while (x < area.width) : (x += 1) {
            if (activeNumber == 0) {
                if (area.isDigit(x, y)) {
                    numberStart = x;
                    activeNumber = area.get(x, y) - '0';
                }
            } else {
                if (area.isDigit(x, y)) {
                    activeNumber = activeNumber * 10 + area.get(x, y) - '0';
                } else {
                    if (area.finalizeNumber(numberStart, x - 1, y, activeNumber))
                        part1 += activeNumber;

                    activeNumber = 0;
                }
            }
        }
        if (activeNumber > 0) {
            if (area.finalizeNumber(numberStart, area.width - 1, y, activeNumber))
                part1 += activeNumber;

            activeNumber = 0;
        }
    }

    var part2: isize = 0;
    for (area.gearValues) |gearValue| {
        if (gearValue > 0)
            part2 += gearValue;
    }

    print("Part 1: {d}\n", .{part1});
    print("Part 2: {d}\n", .{part2});
}
