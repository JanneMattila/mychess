import { ChessBoard } from "./ChessBoard";
import { assert } from "chai";
import { ChessBoardState } from "./ChessBoardState";
import { ChessPlayer } from "./ChessPlayer";
import { ChessBoardPiece } from "./ChessBoardPiece";
import { ChessPiece } from "./ChessPiece";

let board = new ChessBoard();

test("Board initial state has correct amount of available moves", () => {
  // Arrange
  let expected = 20;
  board.initialize();

  // Act
  let actual = board.getAllAvailableMoves().length;

  // Assert
  assert.equal(actual, expected);
});

test("Checkmate in four moves", () => {
  // Arrange
  let expected = 0;
  let expextedState = ChessBoardState.CheckMate;
  let expectedWinner = ChessPlayer.White;

  board.initialize();
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

test("En passant", () => {
  // Arrange
  let expextedCapturePiece = ChessPiece.Pawn;
  let expectedCapturer = ChessPlayer.Black;

  board.initialize();
  board.makeMoveFromString("B2B4");
  board.makeMoveFromString("G7G5");
  board.makeMoveFromString("B4B5");
  board.makeMoveFromString("C7C5");
  board.makeMoveFromString("B5C6");

  // Act
  let capture = board.lastMoveCapture();

  // Assert
  assert.equal(capture?.piece, expextedCapturePiece);
  assert.equal(capture?.player, expectedCapturer);
});
