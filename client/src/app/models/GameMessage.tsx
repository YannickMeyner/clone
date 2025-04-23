import GameState from "./GameState";

export default interface GameMessage {
    action: string;
    gameState?: GameState;
    playerId?: string;
    winnerId?: string;
}
