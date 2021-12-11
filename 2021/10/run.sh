#!/bin/bash
clear
as -g -D -o program.o program.s || exit 1
ld -g -o program program.o || exit 1
./program < input.txt
#gdb program
