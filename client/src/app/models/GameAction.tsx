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
  Left = "Left",
  Right = "Right",
  Down = "Down",
  Drop = "Drop"
}

export interface GameAction {
  ActionType: ActionType;
  Direction?: Direction;
  Rotation?: number;
}