// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Mult.asm

// Multiplies R0 and R1 and stores the result in R2.
// (R0, R1, R2 refer to RAM[0], RAM[1], and RAM[2], respectively.)

//make sure R2 is empty
@R2
M=0;

(LOOP)
//R1 is the counter if its at 0 we go to end
@R1
D=M
@END
D;JLE

//decrement the counter R1 ready for the next iteration
@R1
M=M-1

//increment the accumulator R2, by the value R0
@R0
D=M
@R2
M=D+M

@LOOP
0;JMP

//infinite loop to avoid executing code inserted after the end of the program
(END)
@END
0;JMP
