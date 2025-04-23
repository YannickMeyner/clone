import Block from "./Block";

export default interface Player {
    currentBlock: Block;
    nextBlock?: number[][];
    grid: number[][];
    isGameOver: boolean;
    linesCleared: number;
    userId: string;
    score: number;
}
