// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Fill.asm

// Runs an infinite loop that listens to the keyboard input.
// When a key is pressed (any key), the program blackens the screen,
// i.e. writes "black" in every pixel. When no key is pressed, the
// program clears the screen, i.e. writes "white" in every pixel.

// Put your code here.
(START)

@KBD
D=M
@BLACK
D;JNE

(WHITE)
@R0
M=0
@FILL
0;JMP

(BLACK)
@R0
M=-1

(FILL)
//initialise all variables
@8192
D=A
@remaining
M=D
@SCREEN
D=A
@pixel
M=D

(FILL_LOOP)
//if remaining pixels has hit zero then we jump
@remaining
D=M
@START
D;JLE

//otherwise we shade the pixel
@R0
D=M
@pixel
A=M
M=D

//decrement remaining, increment the pixel
@remaining
M=M-1
@pixel
M=M+1

@FILL_LOOP
0;JMP
