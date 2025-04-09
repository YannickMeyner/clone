"use client";

import React, { useEffect, useState } from "react";
import styles from "./board.module.css";
import useWebSocket from "react-use-websocket";
import GameMessage from "../models/GameMessage";
import { Pixel } from "./Pixel";
import { useEnv } from "../env/provider";

export default function Board() {
    const env = useEnv();
    //@ts-expect-error I'm bad at typescript
    const WS_URL = env.BACKEND_URL;
    const gridWidth = 10;
    const gridHeight = 20;

    const { sendJsonMessage, lastJsonMessage, readyState } = useWebSocket(
        WS_URL, {
        shouldReconnect: () => false, // TODO: change later to true
        onOpen: () => console.log("Connected to server"),
        onClose: () => console.log("Disconnected from server"),
        onError: (event) => console.log("Error", event),
    }
    );

    const [gameLogs, setGameLogs] = useState<string>("");
    const [nextBlockPixels, setNextBlockPixels] = useState<Pixel[]>([]);
    const [nextBlockPixelWidth, setNextBlockPixelsWidth] = useState<number>(1);

    useEffect(() => {
        if (lastJsonMessage !== null) {
            console.debug("Received message", lastJsonMessage);
            const gameMessage: GameMessage = lastJsonMessage as GameMessage;
            setGameLogs(prevLogs => JSON.stringify(gameMessage.action) + "\n" + prevLogs);

            console.log("> Message action:", gameMessage);
            if (gameMessage.action === "UPDATE" && gameMessage.gameState) {

                console.log(" > Game state:", gameMessage.gameState);

                if (gameMessage.gameState.players.self.nextBlock) {
                    setNextBlockPixels(updatePixels(gameMessage.gameState.players.self.nextBlock));
                    setNextBlockPixelsWidth(gameMessage.gameState.players.self.nextBlock.length);
                }

                setSelfPixels(updatePixels(gameMessage.gameState.players.self.grid));
                setOpponentPixels(updatePixels(gameMessage.gameState.players.opponent.grid));
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
            "ActionType": "Join"
        };
        sendJsonMessage(message);
    }

    function sendMoveMessage(direction: "LEFT" | "RIGHT" | "DOWN" | "DROP") {
        const message = direction === "DROP" ? {
            "ActionType": "Drop",
        } : {
            "ActionType": "Move",
            "Direction": direction,
        };

        sendJsonMessage(message);
    }

    function sendRotateMessage() {
        const message = {
            "ActionType": "Rotate",
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

    function keyDown($event: React.KeyboardEvent) {
        if ($event.key === "ArrowRight") {
            console.log("Right");
            if (running) {
                sendMoveMessage("RIGHT");
            }

        } else if ($event.key === "ArrowLeft") {
            console.log("Left");
            if (running) {
                sendMoveMessage("LEFT");
            }

        } else if ($event.key === "ArrowDown") {
            console.log("Down");
            if (running) {
                sendMoveMessage("DOWN");
            }

        } else if ($event.key === "ArrowUp") {
            console.log("Up");
            if (running) {
                sendRotateMessage();
            }

        } else if ($event.key === " ") {
            console.log("Space");
            if (running) {
                sendMoveMessage("DROP");
            }

        } else if ($event.key === "Escape") {
            console.log("Escape");
            setRunning(prevRunning => !prevRunning);
        }
    }


    return (
        <>
            <h3>Gamelogs</h3>
            <div style={{ whiteSpace: "pre-wrap", height: "4rem", overflowY: "scroll" }}>{gameLogs}</div>
            <button onClick={sendInitMessage}>Send Init Message</button>

            <div
                onKeyDown={keyDown} tabIndex={0}
                className={styles.boardContainer}
            >
                {
                    !running && (
                        <div className={styles.overlay}>
                            <h1>Game paused</h1>
                            <p>Press <i>space</i> to resume</p>
                        </div>
                    )
                }

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

                    <div>
                        Nextblock:
                        <div
                            style={{
                                display: "grid",
                                gridTemplateColumns: `repeat(${nextBlockPixelWidth}, 20px)`,
                                gridTemplateRows: `repeat(${nextBlockPixelWidth}, 20px)`,
                                gap: "0px",
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
        </>
    );

}