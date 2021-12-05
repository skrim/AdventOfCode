#!/usr/bin/mumps
    open 1:"input.txt,old"

    set p=""
    for x=1:1:100 set p=p_".........."

    for y=1:1:1000 do
    . set world(y)=p
    . set diagworld(y)=p

    set overlaps=0
    set diagoverlaps=0

    set counter=0

    for  do
    . use 1
    . read line
    . if '$test break
    . use 5
    . if $zperlmatch(line,"^(\d+),(\d+) -> (\d+),(\d+)$") do
    . set x1=$1
    . set y1=$2
    . set x2=$3
    . set y2=$4

    . set counter=counter+1

    . set dx=x2-x1
    . set dy=y2-y1
    . write "line ",counter,": dx ",(x2-x1),", dy ",(y2-y1),!

    . set x=x1
    . set y=y1

    . for  do

    .. if (dx=0)!(dy=0) do
    ... set line=world(y+1)
    ... set char=$Extract(line,x+1)
    ... if char="." do
    .... set world(y+1)=$Extract(line,1,x)_"o"_$Extract(line,x+2,1000)
    ... if char="o" do
    .... set world(y+1)=$Extract(line,1,x)_"#"_$Extract(line,x+2,1000)
    .... set overlaps=overlaps+1

    .. set line=diagworld(y+1)
    .. set char=$Extract(line,x+1)
    .. if char="." do
    ... set diagworld(y+1)=$Extract(line,1,x)_"o"_$Extract(line,x+2,1000)
    .. if char="o" do
    ... set diagworld(y+1)=$Extract(line,1,x)_"#"_$Extract(line,x+2,1000)
    ... set diagoverlaps=diagoverlaps+1

    .. if (x=x2)&(y=y2) break
    .. if x<x2 set x=x+1
    .. if x>x2 set x=x-1
    .. if y<y2 set y=y+1
    .. if y>y2 set y=y-1

    use 5
    write "Step 1: ",overlaps,!
    write "Step 2: ",diagoverlaps,!
