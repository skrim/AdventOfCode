const std = @import("std");
const List = std.ArrayList;

const util = @import("util.zig");
const gpa = util.gpa;

const data = @embedFile("data/day10.txt");

const print = std.debug.print;

const NotAllowed: isize = 0;
const Up: isize = 1;
const Down: isize = 2;
const Left: isize = 3;
const Right: isize = 4;

var height: isize = 0;
var width: isize = 0;
var area: List(Node) = List(Node).init(gpa);

const Node: type = struct {
    shape: u8,
    inLoop: bool = false,

    pub fn setInLoop(self: Node) void {
        self.inLoop = true;
    }

    pub fn findExit(self: Node, entry: isize) isize {
        const shapes = [_]u8 { '|', '|', '-', '-', 'L', 'L', 'J', 'J', '7', '7', 'F', 'F' };
        const entryDirs = [_]isize { Up, Down, Left, Right, Down, Left, Down, Right, Up, Right, Up, Left };
        const resultDirs = [_]isize { Up, Down, Left, Right, Right, Up, Left, Up, Left, Down, Right, Down };

        for (shapes, entryDirs, resultDirs) |shape, entryDir, resultDir| {
            if (self.shape == shape and entry == entryDir)
                return resultDir;
        }
        return NotAllowed;
    }

    pub fn init(shape: u8) Node {
        return Node { .shape = shape };
    }

    pub fn determineDirection(dir1: isize, dir2: isize) u8 {
        if (dir1 == Down and dir2 == Up) return '|';
        if (dir1 == Down and dir2 == Right) return 'F';
        if (dir1 == Down and dir2 == Left) return '7';
        if (dir2 == Up and dir1 == Right) return 'L';
        if (dir2 == Up and dir1 == Left) return 'J';
        if (dir1 == Right and dir2 == Left) return '-';
        unreachable;
    }
};

var externalNode = Node.init('.');

const Crawler: type = struct {
    lastDirection: isize,
    x: isize,
    y: isize,
    moves: isize,

    pub fn move(self: *Crawler) void {
        var node = getNode(self.x, self.y);
        node.inLoop = true;
        const direction = node.findExit(self.lastDirection);

        switch (direction) {
            Up => self.y -= 1,
            Down => self.y += 1,
            Left => self.x -= 1,
            Right => self.x += 1,
            else => unreachable
        }
        self.lastDirection = direction;
        self.moves += 1;
    }
};

pub fn getNode(x: isize, y: isize) *Node {
    if (x < 0 or y < 0 or x >= width or y >= height)
        return &externalNode;

    const ux = @as(usize, @intCast(x));
    const uy = @as(usize, @intCast(y));
    const uw = @as(usize, @intCast(width));

    return &area.items[uy * uw + ux];
}

pub fn initCrawler(x: isize, y: isize, counter: isize) Crawler {
    const dxs = [_]isize { 0, 0, 1, -1 };
    const dys = [_]isize { 1, -1, 0, 0 };
    const dirs = [_]isize { Down, Up, Right, Left };

    var c : isize = counter;
    for (dxs, dys, dirs) |dx, dy, dir| {
        var node = getNode(x + dx, y + dy);
        if (node.findExit(dir) != NotAllowed) {
            c -= 1;
            if (c == 0)
                return Crawler { .lastDirection = dir, .x = x + dx, .y = y + dy, .moves = 1 };
        }
    }
    unreachable;
}

fn countInsideNodes() isize {
    var result : isize = 0;
    var y: isize = 0;
    while (y < height) {
        var inside: bool = false;
        var entry: u8 = ' ';

        var x: isize = 0;
        while (x < width) {
            const node = getNode(x, y);
            const shape: u8 = node.shape;

            if (node.inLoop) {
                if (shape == '|') {
                    inside = !inside;
                } else if (shape != '-') {
                    if (shape == 'L' or shape == 'F')
                        entry = shape;

                    if (entry == 'L' and shape == '7') inside = !inside;
                    if (entry == 'F' and shape == 'J') inside = !inside;
                }
            }
            if (!node.inLoop and inside)
                result += 1;

            x += 1;
        }
        y += 1;
    }
    return result;
}

pub fn main() !void {
    var startX: isize = 0;
    var startY: isize = 0;

    var lineIterator = std.mem.tokenize(u8, data, "\r\n");
    while (lineIterator.next()) |line| {
        width = 0;
        for (line) |c| {
            area.append(Node.init(c)) catch unreachable;

            if (c == 'S') {
                startX = width;
                startY = height;
            }
            width += 1;
        }
        height += 1;
    }

    var crawler1 = initCrawler(startX, startY, 1);
    var crawler2 = initCrawler(startX, startY, 2);

    var startNode = getNode(startX, startY);
    const startDir = Node.determineDirection(crawler1.lastDirection, crawler2.lastDirection);
    startNode.shape = startDir;
    startNode.inLoop = true;

    while (crawler1.x != crawler2.x or crawler1.y != crawler2.y) {
        crawler1.move();
        crawler2.move();
    }

    const part1: isize = (&crawler1).moves;
    crawler1.move(); // mark last as in loop

    const part2: isize = countInsideNodes();

    print("Part1: {d}\n", .{part1});
    print("Part2: {d}\n", .{part2});
}
