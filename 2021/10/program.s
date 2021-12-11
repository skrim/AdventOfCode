.equ BUFFERSIZE,    20000
.equ STACKSIZE,     1000
.equ NUMBERBUFFER,  100
.equ SORTBUFFER,    1000

.equ STDIN,  0
.equ STDOUT, 1
.equ READ,   63
.equ WRITE,  64
.equ EXIT,   93

.equ OPEN_ROUND,            0x28
.equ CLOSE_ROUND,           0x29
.equ SCORE_ROUND,           3      // 11
.equ REPAIRSCORE_ROUND,     1

.equ OPEN_SQUARE,           0x5b
.equ CLOSE_SQUARE,          0x5d
.equ SCORE_SQUARE,          57     // 111001
.equ REPAIRSCORE_SQUARE,    2

.equ OPEN_CURLY,            0x7b
.equ CLOSE_CURLY,           0x7d
.equ SCORE_CURLY,           1197   // 10010101101
.equ REPAIRSCORE_CURLY,     3

.equ OPEN_ANGLE,            0x3c
.equ CLOSE_ANGLE,           0x3e
.equ SCORE_ANGLE,           25137  // 110001000110001
.equ REPAIRSCORE_ANGLE,     4

.data
syntaxScore:		.asciz "Syntax score: "
repairScore:		.asciz "Repair score: "
carriageReturn:  	.asciz "\n"

//Read Buffer
.bss
buffer:         .skip  BUFFERSIZE
stack:          .skip  STACKSIZE
numberBuffer:   .skip  NUMBERBUFFER
sortBuffer:     .skip  SORTBUFFER

.text
.global _start

quadSyntaxScore:        .quad   syntaxScore
quadRepairScore:        .quad   repairScore
quadCarriageReturn:	    .quad   carriageReturn

quadBuffer:          	.quad   buffer          // read buffer
quadStack:              .quad   stack           // token stack
quadNumberBuffer:       .quad   numberBuffer    // for writing numbers
quadSortBuffer:         .quad   sortBuffer      // repair scores

// x10 - input address
// x11 - input offset
// x12 - stack address
// x13 - stack offset

// w15 - score of line
// w16 - total score (part 1)
// x17 - repair buffer count
// x18 - repair score of line


// w9 - char to compare input
pushToken:
    ldrb w1, [x10, x11]
    sub w1, w1, w9
    cbnz w1, finishPushToken
    mov w2, w9
    strb w2, [x12, x13]
    add x13, x13, #1

    finishPushToken:
        ret

// w9 - char to compare input
// w8 - expected value in stack
// w7 - score
popToken:
    ldrb w1, [x10, x11]
    sub w1, w1, w9
    cbnz w1, finishPopToken // not character we want

    sub x22, x13, 1
    ldrb w1, [x12, x22]
    sub w1, w1, w8
    cbz w1, rewindStack

    // invalid stack
    cbnz w15, finishPopToken // line already has score

    mov w15, w7
    b finishPopToken

    rewindStack:
        mov w22, #0
        sub x13, x13, 1
        strb w22, [x12, x13]

    finishPopToken:
        ret

readLine:
    mov w9, OPEN_ANGLE
    bl pushToken

    mov w9, OPEN_ROUND
    bl pushToken

    mov w9, OPEN_SQUARE
    bl pushToken

    mov w9, OPEN_CURLY
    bl pushToken

    mov w9, CLOSE_ANGLE
    mov w8, OPEN_ANGLE
    mov w7, SCORE_ANGLE
    bl popToken

    mov w9, CLOSE_ROUND
    mov w8, OPEN_ROUND
    mov w7, SCORE_ROUND
    bl popToken

    mov w9, CLOSE_SQUARE
    mov w8, OPEN_SQUARE
    mov w7, SCORE_SQUARE
    bl popToken

    mov w9, CLOSE_CURLY
    mov w8, OPEN_CURLY
    mov w7, SCORE_CURLY
    bl popToken


    add x11, x11, 1

    ldrb w1, [x10, x11]

    cbz w1, complete            // end of input
    sub w1,w1,0x0a
    cbz w1, completeLine
    b readLine

completeLine:
    cbnz w15, calculateSyntaxScore

    // repair
    mov x18, #0

    mov x22, #0 // clear stack value
    mov x24, #5 // multiplier
    repairStackLoop:
        sub x13, x13, 1
        mul x18, x18, x24

        ldrb w1, [x12, x13]
        sub w1, w1, OPEN_ROUND
        cbnz w1, endRoundRepair
        add x18, x18, REPAIRSCORE_ROUND
        endRoundRepair:

        ldrb w1, [x12, x13]
        sub w1, w1, OPEN_SQUARE
        cbnz w1, endSquareRepair
        add x18, x18, REPAIRSCORE_SQUARE
        endSquareRepair:

        ldrb w1, [x12, x13]
        sub w1, w1, OPEN_CURLY
        cbnz w1, endCurlyRepair
        add x18, x18, REPAIRSCORE_CURLY
        endCurlyRepair:

        ldrb w1, [x12, x13]
        sub w1, w1, OPEN_ANGLE
        cbnz w1, endAngleRepair
        add x18, x18, REPAIRSCORE_ANGLE
        endAngleRepair:

        strb w22, [x12, x13]
        cbnz x13, repairStackLoop

    add x19, x19, x18

    mov x29, x30
        ldr x0, quadRepairScore
        bl writeMessage

        ldr x0, quadCarriageReturn
        bl writeMessage

        mov x9, x18
        bl writeNumber

        mov x9, x19
        bl writeNumber

        ldr x0, quadCarriageReturn
        bl writeMessage

        bl insertSorted
    mov x30, x29

    b readLine

insertSorted:
    ldr x0, quadSortBuffer

    // x18 - incoming value

    mov x1, #0 // index

    findPos:
        ldr x2, [x0]
        cbz x2, insertToPos
        add x0, x0, #8
        cmp x2, x18
        bgt findPos
        sub x0, x0, #8

    insertToPos:
        ldr x3, [x0] // swap
        str x18, [x0]

        cbz x3, insertDone
        mov x18, x3
        add x0, x0, #8
        b insertToPos

    insertDone:

    add x17, x17, #1
    ret

calculateSyntaxScore:
    add w16, w16, w15

    mov x29, x30

    ldr x0, quadSyntaxScore
    bl writeMessage

    ldr x0, quadCarriageReturn
    bl writeMessage

    mov w9, w15
    bl writeNumber

    mov w9, w16
    bl writeNumber

    ldr x0, quadCarriageReturn
    bl writeMessage

    mov x30, x29
    mov w15, 0

    mov w22, #0
    strb w22, [x12, x13]
    clearStackLoop:
        sub x13, x13, 1
        strb w22, [x12, x13]
        cbnz x13, clearStackLoop

    b readLine

// x0 - *message
writeMessage:
    mov x2, 0               // length for syscall

    checkSize:
        ldrb w1, [x0, x2]
        add x2, x2, #1
        cbz w1, output
        b checkSize

    output:
        mov x1, x0          // string address for syscall
        mov x0, STDOUT
        mov x8, WRITE
        svc 0
        ret

// x9 - number to write
writeNumber:

    ldr x1, quadNumberBuffer
    mov x2, 0 // length for syscall
    mov x3, 0x4000000000000000

    writeNumberLoop:
        and x4, x9, x3

        mov w8, 48
        cbz x4, printNumber
        mov w8, 49

        printNumber:
        strb w8, [x1, x2]

        add x2, x2, #1
        asr x3, x3, #1

        cbnz x3, writeNumberLoop

    mov x0, STDOUT
    mov x8, WRITE
    svc 0

    ldr x0, quadCarriageReturn
    b writeMessage

_start:
    // read from stdin
    mov x0, STDIN
    ldr x1, quadBuffer
    mov x2, BUFFERSIZE
    mov x8, READ
    svc 0

    ldr x10, quadBuffer
    ldr x12, quadStack

    b readLine

complete:
    ldr x0, quadCarriageReturn
    bl writeMessage

    mov w9, w16
    bl writeNumber

    // offset of middle value in sort buffer
    ldr x0, quadSortBuffer
    mov x2, 8
    mul x1, x17, x2
    mov x2, 2
    udiv x1, x1, x2
    sub x1, x1, 4
    add x0, x0, x1
    ldr x9, [x0]
    bl writeNumber

end:
    mov x0, #0      // return code
    mov x8,EXIT
    svc 0
