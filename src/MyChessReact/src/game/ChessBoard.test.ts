import { ChessBoard } from "./ChessBoard";
import { assert } from "chai";
import { ChessBoardState } from "./ChessBoardState";
import { ChessPlayer } from "./ChessPlayer";

let board = new ChessBoard();

test("board initial state has correct amount of available moves", () => {
  // Arrange
  let expected = 20;

  // Act
  let actual = board.getAllAvailableMoves().length;

  // Assert
  assert.equal(actual, expected);
});

test("board initial state has correct amount of available moves", () => {
  // Arrange
  let expected = 0;
  let expextedState = ChessBoardState.CheckMate;
  let expectedWinner = ChessPlayer.White;

  board.makeMoveFromString("E2E4"); // White Pawn
  board.makeMoveFromString("A7A6"); // Black Pawn
  board.makeMoveFromString("F1C4"); // White Bishop
  board.makeMoveFromString("H7H6"); // Black Pawn
  board.makeMoveFromString("D1F3"); // White Queen
  board.makeMoveFromString("A6A5"); // Black Pawn
  board.makeMoveFromString("F3F7"); // White Queen

  // Act
  let actual = board.getAllAvailableMoves().length;
  let actualState = board.GetBoardState();
  let actualPlayer = board.lastMove()?.player;

  // Assert
  assert.equal(actual, expected);
  assert.equal(actualState, expextedState);
  assert.equal(actualPlayer, expectedWinner);
});
