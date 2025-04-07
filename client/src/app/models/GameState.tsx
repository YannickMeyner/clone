import Player from "./Player";

export default interface GameState {
    IsGameActive: boolean;
    players: Players;
}

export interface Players {
    opponent: Player;
    self: Player;
}