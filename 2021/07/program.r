#!/usr/bin/r3 -s
REBOL [Title: "AoC 2021-12-07"]

data: read/string %input.txt
lines: split data ","

total: 0
count: 0
min: 99999
max: 0
foreach line lines [
    if line <> "" [
        value: to-integer line
        total: total + value
        count: count + 1
        if value < min [ min: value ]
        if value > max [ max: value ]
    ]
]

mintotalfuel1: 99999999999
mintotalfuel2: 99999999999

for testvalue min max 1 [
    fuel1: 0
    fuel2: 0
    foreach line lines [
        if line <> "" [
            value: to-integer line

            value: testvalue - value
            value: abs value
           
            fuel1: fuel1 + value

            value2: ( value * (value + 1) ) / 2
            fuel2: fuel2 + value2
        ]
    ]
    if fuel1 < mintotalfuel1 [
        mintotalfuel1: fuel1
        bestmatch1: testvalue
    ]
    if fuel2 < mintotalfuel2 [
        mintotalfuel2: fuel2
        bestmatch2: testvalue
    ]
]
print rejoin [ "Step 1: " mintotalfuel1 ", best height " bestmatch1 ]
print rejoin [ "Step 2: " mintotalfuel2 ", best height " bestmatch2 ]
