import std.range, std.stdio, std.array, std.regex, std.algorithm, std.conv;

class Range {
    int from, to;

    this(int f, int t) {
        from = f;
        to = t;
    }

    long magnitude() => to - from + 1;

    bool intersects(Range other) => other.to >= from && other.from <= to;

    Range[] split(Range other) {
        auto result = new Range[0];
        if (other.from <= from && other.to >= to) {
            result ~= new Range(from, to);
        } else if (other.from <= from && other.to >= from) {
            result ~= new Range(from, other.to);
            result ~= new Range(other.to + 1, to);
        } else if (other.from > from && other.to < to) {
            result ~= new Range(from, other.from - 1);
            result ~= new Range(other.from, other.to);
            result ~= new Range(other.to + 1, to);
        } else if (other.to >= to && other.from <= to) {
            result ~= new Range(from, other.from - 1);
            result ~= new Range(other.from, to);
        }
        return result;
    }    
}

class Area {
    bool state;
    Range x, y, z;

    this(bool st, int x1, int x2, int y1, int y2, int z1, int z2) {
        state = st;
        x = new Range(x1, x2);
        y = new Range(y1, y2);
        z = new Range(z1, z2);
    }

    long volume() => x.magnitude() * y.magnitude() * z.magnitude();

    bool intersects(Area other) => x.intersects(other.x) && y.intersects(other.y) && z.intersects(other.z);

    Area[] subtract(Area other) {
        auto result = new Area[0];
        foreach (xp; x.split(other.x)) {
            auto a1 = new Area(state, xp.from, xp.to, y.from, y.to, z.from, z.to);
            foreach (yp; a1.y.split(other.y)) {
                auto a2 = new Area(state, a1.x.from, a1.x.to, yp.from, yp.to, a1.z.from, a1.z.to);
                foreach (zp; a2.z.split(other.z)) {
                    auto a3 = new Area(state, a2.x.from, a2.x.to, a2.y.from, a2.y.to, zp.from, zp.to);
                    if (!a3.intersects(other)) result ~= a3;
                }
            }
        }
        return result;
    }
}

long calculate(Area[] areas, Area subset) {
    auto result = new Area[0];
    foreach (incoming; areas.filter!(a => subset is null || subset.intersects(a))) {
        int i = 0;
        while (i < result.length) {
            auto existing = result[i];
            if (!existing.intersects(incoming)) {
                i++;
                continue;
            }
            result = result.remove(i) ~ existing.subtract(incoming);
        }
        result ~= incoming;
    }
    return sum(result.filter!(a => a.state).map!(a => a.volume()));
}

void main() {
    auto areas = new Area[0];
    foreach (line; File("input.txt").byLine()) {
        auto vals = new int[0];
        foreach(m; matchAll(line, regex(r"(\-?\d+)"))) vals ~= to!int(m.hit);
        areas ~= new Area(startsWith(line, "on"), vals[0], vals[1], vals[2], vals[3], vals[4], vals[5]);
    }
    writefln("Part 1: %d", calculate(areas, new Area(false, -50, 50, -50, 50, -50, 50)));
    writefln("Part 2: %d", calculate(areas, null));
}