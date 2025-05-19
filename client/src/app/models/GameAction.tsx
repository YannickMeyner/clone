"use client";

export enum ActionType {
  Move = "Move",
  Rotate = "Rotate",
  Drop = "Drop",
  Start = "Start",
  Stop = "Stop",
  Join = "Join",
  Init = "Init"
}

export enum Direction {
  Left = "Left",    // Könnte auch "LEFT" sein, je nach Server-Erwartung
  Right = "Right",  // Könnte auch "RIGHT" sein
  Down = "Down",    // Könnte auch "DOWN" sein
  Drop = "Drop"     // Könnte auch "DROP" sein
}

export interface GameAction {
  ActionType: ActionType;
  Direction?: Direction;
  Rotation?: number;
}