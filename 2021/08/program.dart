import 'dart:io';
import 'dart:collection';
import 'dart:core';
import 'dart:convert';

const numberSegments = [
    [ 0, 1, 2,    4, 5, 6 ],
    [       2,       5    ],
    [ 0,    2, 3, 4,    6 ],
    [ 0,    2, 3,    5, 6 ],
    [    1, 2, 3,    5    ],
    [ 0, 1,    3,    5, 6 ],
    [ 0, 1,    3, 4, 5, 6 ],
    [ 0,    2,       5    ],
    [ 0, 1, 2, 3, 4, 5, 6 ],
    [ 0, 1, 2, 3,    5, 6 ]
];

int part1 = 0;
int part2 = 0;

void main() async {
    final file = File("input.txt");
    Stream<String> lines = file.openRead().transform(utf8.decoder).transform(LineSplitter());

    await for (var line in lines) solve(line);

    print("Part 1: $part1");
    print("Part 2: $part2");
}

Set<String> frequencies(Iterable<String> list, int target) {
    var result = HashMap<String, int>();
    for (var item in list) result[item] = (result[item] ?? 0) + 1;
    result.removeWhere((key, value) => value != target);
    return result.keys.toSet();
}

void solve(String item) {
    var parts = item.split('|');
    var options = parts[0].trim().split(' ').map((t) => t.split("").toSet()).toList();
    options.sort( (a, b) => a.length - b.length);

    var expanded = options.expand((e) => e);

    var values = parts[1].trim().split(' ').map((t) => t.split("").toSet()).toList();

    var segments = List<String>.filled(7, "");

    segments[1] = frequencies(expanded, 6).single;
    segments[4] = frequencies(expanded, 4).single;
    segments[5] = frequencies(expanded, 9).single;

    var segments36 = frequencies(expanded, 7);
    segments[3] = segments36.intersection(options[2]).single;
    segments[6] = segments36.difference(options[2]).single;

    var segments02 = frequencies(expanded, 8);
    segments[0] = segments02.difference(options[0]).single;
    segments[2] = segments02.intersection(options[0]).single;

    var numbers = numberSegments.map( (list) => list.map((i) => segments[i]).toSet() ).toList();

    var total = 0;
    for (var value in values) {
        total *= 10;
        var number = numbers.indexOf(numbers.where( (n) => n.containsAll(value) && value.containsAll(n)).single);
        if (number == 1 || number == 4 || number == 7 || number == 8) part1++;
        total += number;
    }
    part2 += total;
}
