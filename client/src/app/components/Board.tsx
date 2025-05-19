"use client";

import React, { useEffect, useState } from "react";
import styles from "./board.module.css";
import useWebSocket from "react-use-websocket";
import GameMessage from "../models/GameMessage";
import { Pixel } from "./Pixel";
import KeyboardLabel from "./keyboardLabel";
import { useAuth } from "../context/auth-context";
import { ActionType, Direction } from "../models/GameAction";

export default function Board() {
    const wsBaseUrl = process.env.NEXT_PUBLIC_WS_BASE_URL ?? 'ws://localhost:5001';
    const { token } = useAuth();
    // token anhängen, damit die UserId extracted werden kann im Server
    const WS_URL = `${wsBaseUrl}?token=${token}`;
    const gridWidth = 10;
    const gridHeight = 20;

    const { sendJsonMessage, lastJsonMessage, readyState } = useWebSocket(
        WS_URL, {
        shouldReconnect: () => true,
        onOpen: () => console.log("Connected to server"),
        onClose: () => console.log("Disconnected from server"),
        onError: (event) => console.log("Error", event),
    }
    );
    const [nextBlockPixels, setNextBlockPixels] = useState<Pixel[]>([]);
    const [nextBlockPixelWidth, setNextBlockPixelsWidth] = useState<number>(1);
    const [playerId, setPlayerId] = useState<string>("");
    const [winner, setWinner] = useState<'Running' | 'You win' | 'Opponent win'>('Running');
    const [gameState, setGameState] = useState<'WAITING' | 'READY' | 'PLAYING' | 'GAME_OVER'>('WAITING');

    useEffect(() => {
        if (lastJsonMessage !== null) {
            console.debug("Received message", lastJsonMessage);
            const gameMessage: GameMessage = lastJsonMessage as GameMessage;

            console.log("> Message action:", gameMessage);
            if (gameMessage.action === "UPDATE" && gameMessage.gameState) {
                setGameState('PLAYING');

                console.log(" > Game state:", gameMessage.gameState);

                if (gameMessage.gameState.players.self.nextBlock) {
                    setNextBlockPixels(updatePixels(gameMessage.gameState.players.self.nextBlock));
                    setNextBlockPixelsWidth(gameMessage.gameState.players.self.nextBlock.length);
                }

                setSelfPixels(updatePixels(gameMessage.gameState.players.self.grid));
                setOpponentPixels(updatePixels(gameMessage.gameState.players.opponent.grid));
            } else if (gameMessage.action === "GAME_OVER") {
                setGameState('GAME_OVER');
                setWinner(gameMessage.winnerId === playerId ? 'You win' : 'Opponent win');
            } else if (typeof gameMessage.action === 'number') {
                setGameState('READY');
                setPlayerId(gameMessage.playerId ?? '-1');
            }

        }
    }, [lastJsonMessage]);

    function updatePixels(grid: number[][]) {
        const updated_pixels: Pixel[] = [];
        for (let i = 0; i < grid.length; i++) {
            for (let j = 0; j < grid[i].length; j++) {
                const pixel = grid[i][j];
                if (pixel) {
                    updated_pixels.push(new Pixel(pixel, {}));
                } else {
                    updated_pixels.push(new Pixel(0, {}));
                }
            }
        }
        return updated_pixels;
    }

    useEffect(() => {
        console.log("Sending message: readyState:", readyState); // UNINSTANTIATED = -1, CONNECTING = 0, OPEN = 1, CLOSING = 2, CLOSED = 3
    }, [readyState]);

    
    function sendInitMessage() {
        const message = {
            "ActionType": ActionType.Join
        };
        sendJsonMessage(message);
    }

    function sendMoveMessage(direction: Direction) {
        if (direction === Direction.Drop) {
            const message = {
                "ActionType": ActionType.Drop,
            };
            sendJsonMessage(message);
        } else {
            const message = {
                "ActionType": ActionType.Move,
                "Direction": direction,
            };
            sendJsonMessage(message);
        }
    }

    function sendRotateMessage() {
        const message = {
            "ActionType": ActionType.Rotate,
        };
        sendJsonMessage(message);
    }

    const [running, setRunning] = useState<boolean>(true);

    const [selfPixels, setSelfPixels] = useState<Pixel[]>([
        new Pixel(1, {}),
        new Pixel(2, {}),
        new Pixel(3, {}),
        new Pixel(4, {}),
        new Pixel(5, {}),
        new Pixel(6, {}),
        new Pixel(7, {}),
        new Pixel(8, {}),
        new Pixel(9, {}),
        new Pixel(10, {}),
    ]);

    const [opponentPixels, setOpponentPixels] = useState<Pixel[]>([
        new Pixel(1, {}),
        new Pixel(2, {}),
        new Pixel(3, {}),
        new Pixel(4, {}),
        new Pixel(5, {}),
        new Pixel(6, {}),
        new Pixel(7, {}),
        new Pixel(8, {}),
        new Pixel(9, {}),
        new Pixel(10, {}),
    ]);

    function resetGame() {
        setGameState('WAITING');
        setWinner('Running');
        setSelfPixels([]);
        setOpponentPixels([]);
        setNextBlockPixels([]);
        setNextBlockPixelsWidth(1);
        setPlayerId("");
    }

    function keyDown($event: {key: string}) {
        if ($event.key === "ArrowRight") {
            console.log("Right");
            if (running) {
                sendMoveMessage(Direction.Right);
            }

        } else if ($event.key === "ArrowLeft") {
            console.log("Left");
            if (running) {
                sendMoveMessage(Direction.Left);
            }

        } else if ($event.key === "ArrowDown") {
            console.log("Down");
            if (running) {
                sendMoveMessage(Direction.Down);
            }

        } else if ($event.key === "ArrowUp") {
            console.log("Up");
            if (running) {
                sendRotateMessage();
            }

        } else if ($event.key === " ") {
            console.log("Space");
            if (running) {
                sendMoveMessage(Direction.Drop);
            }

        } else if ($event.key === "Escape") {
            console.log("Escape");
            setRunning(prevRunning => !prevRunning);
        } else if ($event.key === "R" || $event.key === "r") {
            console.log("[R] Reset");
            resetGame();
        }
    }


    return (
        <>
            <div
                onKeyDown={keyDown} tabIndex={0}
                className={styles.boardContainer}
            >
                {
                    gameState === 'GAME_OVER' && (
                        <div className={styles.overlay}>
                            <h1>Game over</h1>
                            <p style={{ color: winner === 'You win' ? '#d3fc19' : 'RED' }}>{winner}</p>
                            <KeyboardLabel label="R" description="Reset" onClick={() => keyDown({key: "R"})} />
                        </div>
                    )
                }
                <div>
                    <div className={styles.infoContainer}>
                        <p>Gamestate: {gameState}</p>
                        <button
                            disabled={gameState === 'READY' || gameState === 'PLAYING'}
                            onClick={sendInitMessage}
                            className={styles.button}
                        >Ready</button>
                    </div>

                    <div className={styles.keyboardControlsContainer}>
                        <div className={styles.arrowKeysContainer}>
                            <div className={styles.arrowTop}>
                                <KeyboardLabel label="Up" description="Rotate" onClick={() => keyDown({key: "ArrowUp"})} />
                            </div>
                            <div className={styles.arrowMiddle}>
                                <KeyboardLabel label="Left" description="Move left" onClick={() => keyDown({key: "ArrowLeft"})} />
                                <KeyboardLabel label="Down" description="Move down" onClick={() => keyDown({key: "ArrowDown"})} />
                                <KeyboardLabel label="Right" description="Move right" onClick={() => keyDown({key: "ArrowRight"})} />
                            </div>
                        </div>
                        <div className={styles.otherKeysContainer}>
                            <KeyboardLabel label="Space" description="Drop" widthMultiplier={2} onClick={() => keyDown({key: " "})} />
                        </div>
                    </div>

                </div>

                <div>
                    <div
                        className={styles.board}
                    >


                        {
                            selfPixels && (

                                <div
                                    style={{
                                        display: "grid",
                                        gridTemplateColumns: `repeat(${gridWidth}, 20px)`,
                                        gridTemplateRows: `repeat(${gridHeight}, 20px)`,
                                        gap: "0px",
                                    }}
                                >
                                    {
                                        selfPixels.map((pixel, index) => {
                                            return pixel.render(index);
                                        })
                                    }

                                </div>
                            )
                        }


                    </div>
                    <div
                        className={styles.separator}
                    ></div>
                    <div
                        className={styles.board}
                    >
                        {
                            opponentPixels && (
                                <div
                                    style={{
                                        display: "grid",
                                        gridTemplateColumns: `repeat(${gridWidth}, 20px)`,
                                        gridTemplateRows: `repeat(${gridHeight}, 20px)`,
                                        gap: "0px",
                                    }}
                                >
                                    {
                                        opponentPixels.slice().reverse().map((pixel, index) => {
                                            return pixel.render(index);
                                        })
                                    }
                                </div>
                            )
                        }
                    </div>
                </div>

                <div>
                    <p>Nextblock:</p>
                    <div
                        className={styles.nextblockContainer}
                        style={{
                            gridTemplateColumns: `repeat(${nextBlockPixelWidth}, 20px)`,
                            gridTemplateRows: `repeat(${nextBlockPixelWidth}, 20px)`,
                        }}
                    >
                        {
                            nextBlockPixels.map((pixel, index) => {
                                return pixel.render(index);
                            })
                        }
                    </div>
                </div>
            </div>
        </>
    );

}