import Block from "./Block";

export default interface Player {
    currentBlock: Block;
    nextBlock?: number[][];
    grid: number[][];
    isGameOver: boolean;
    linesCleared: number;
    playerId: string;
    score: number;
}
