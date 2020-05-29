import { ChessBoard } from "./ChessBoard";
import { ChessMove } from "./ChessMove";
import { ChessBoardPiece } from "./ChessBoardPiece";
import { ChessPiece } from "./ChessPiece";
import { ChessBoardState } from "./ChessBoardState";
import { MyChessGame } from "../models/MyChessGame";
import { setTimeout } from "timers";
import { ChessPlayer } from "./ChessPlayer";

export class ChessBoardView {
    private board: ChessBoard = new ChessBoard();
    private previousAvailableMoves: ChessMove[] = []
    private game: MyChessGame = new MyChessGame();
    private currentMoveNumber: number = 0;
    private waitingForConfirmation = false;

    private imagesLoaded = 0;
    private imagesToLoad = -1;
    private images: HTMLImageElement[] = [];

    private touch?: Touch;

    public initialize(currentPlayerTurn: boolean = false) {
        // Start preparing the board
        this.board = new ChessBoard();
        this.board.initialize();
        this.previousAvailableMoves = [];

        // Update game board to the screen
        this.drawBoard();

        // Add "de-selection" when clicking outside the board
        document.addEventListener("click", (event: MouseEvent) => {
            if (!event.defaultPrevented) {
                this.pieceSelected("9-9");
            }
        });

        document.addEventListener('keyup', (event) => {
            console.log(event.keyCode);
            switch (event.keyCode) {
                case 36: // Home
                    this.firstMove();
                    break;
                case 37: // LeftArrow
                case 40: // DownArrow
                    this.previousMove();
                    break;
                case 39: // RightArrow
                case 38: // UpArrow
                    this.nextMove();
                    break;
                case 35: // End
                    this.lastMove();
                    break;
                default:
                    break;
            }
        });

        document.addEventListener('touchstart', (event) => {
            this.touch = undefined;
            if (event.changedTouches.length === 1) {
                this.touch = event.changedTouches[0];
            }
        });

        document.addEventListener('touchend', (event) => {
            if (this.touch !== undefined &&
                event.changedTouches.length === 1) {
                const delta = 40;
                let touchEnd = event.changedTouches[0];
                if (Math.abs(touchEnd.clientY - this.touch.clientY) > delta) {
                    if (touchEnd.clientY < this.touch.clientY - delta) {
                        this.firstMove();
                    }
                    else {
                        this.lastMove();
                    }
                }
                else if (Math.abs(touchEnd.clientX - this.touch.clientX) > delta) {
                    if (touchEnd.clientX < this.touch.clientX - delta) {
                        this.previousMove();
                    }
                    else {
                        this.nextMove();
                    }
                }
            }
        });
    }

    private loadImages() {
        const files = [
            "Empty",
            "PawnWhite", "BishopWhite", "KnightWhite", "RookWhite", "QueenWhite", "KingWhite",
            "PawnBlack", "BishopBlack", "KnightBlack", "RookBlack", "QueenBlack", "KingBlack",
        ];
        this.imagesToLoad = files.length;

        for (let i = 0; i < files.length; i++) {
            let file = files[i];
            let img = new Image();
            img.onload = (evt) => {
                this.imagesLoaded++;
                if (this.imagesLoaded === this.imagesToLoad) {
                    this.drawBoard();
                }
            };
            img.src = "/images/" + file + ".svg";
            this.images[i] = img;
        }
    }

    public async load(url: string, friendID: string) {
        this.initialize();
        console.log("local game");
        this.loadImages();
        this.game = new MyChessGame();

        if (url.indexOf("/new") === -1) {
            console.log("get form url " + url);

            try {
                const response = await fetch(url);
                const data = await response.json();
                this.game = JSON.parse(data) as MyChessGame;

                let animatedMoves = Math.min(3, this.game.moves.length);
                let moves = this.game.moves.length - animatedMoves;
                this.currentMoveNumber = this.makeNumberOfMoves(this.game, moves);
                setTimeout(() => {
                    this.animateNextMove();
                }, 1000);
            } catch (error) {
                console.log(error);
                // $("#errorText").text(textStatus);
                // $("#errorDialog").dialog();
            }
        }
    }

    private makeNumberOfMoves(game: MyChessGame, movesCount: number): number {
        this.initialize(true);
        this.game = game;

        let count = Math.min(game.moves.length, movesCount);
        console.log("going to make " + count + " moves");
        for (let i = 0; i < count; i++) {
            let move = game.moves[i];
            let promotion = move.promotion !== null ? move.promotion : "";
            this.makeMove(move.move, promotion);
        }
        this.setBoardStatus(count, game.moves.length);
        let move = game.moves[count - 1];
        this.setThinkTime(count, move.time);
        this.setComment(move.comment);
        this.drawBoard();

        return count;
    }

    public drawBoard() {
        if (this.imagesLoaded !== this.imagesToLoad) {
            console.log(`images not yet loaded: ${this.imagesLoaded} / ${this.imagesToLoad}`);
            return;
        }
        console.log("drawBoard");

        // Update game board table
        let table = document.getElementById("table-game") as HTMLTableElement;
        table.innerHTML = "";

        let lastMove = this.board.lastMove();
        let lastMoveCapture = this.board.lastMoveCapture();

        for (let row = 0; row < ChessBoard.BOARD_SIZE; row++) {

            let rowElement = document.createElement("tr") as HTMLTableRowElement;
            table.appendChild(rowElement);

            for (let column = 0; column < ChessBoard.BOARD_SIZE; column++) {

                let piece: ChessBoardPiece = this.board.getPiece(column, row);
                let cell: HTMLTableCellElement = document.createElement("td") as HTMLTableCellElement;

                rowElement.appendChild(cell);

                cell.id = "" + row + "-" + column;
                cell.addEventListener('click', (evt) => {
                    console.log("onCellClick event");
                    let element = evt.currentTarget as HTMLElement;
                    this.pieceSelected(element.id);
                    evt.preventDefault();
                });

                let imageIndex;
                switch (piece.piece) {
                    case ChessPiece.Pawn:
                        imageIndex = 1;
                        break;
                    case ChessPiece.Bishop:
                        imageIndex = 2;
                        break;
                    case ChessPiece.Knight:
                        imageIndex = 3;
                        break;
                    case ChessPiece.Rook:
                        imageIndex = 4;
                        break;
                    case ChessPiece.Queen:
                        imageIndex = 5;
                        break;
                    case ChessPiece.King:
                        imageIndex = 6;
                        break;
                    default:
                        imageIndex = 0;
                        break;
                }
                if (piece.player === ChessPlayer.Black) {
                    imageIndex += 6;
                }

                cell.appendChild(this.images[imageIndex].cloneNode());

                if ((row + column) % 2 === 0) {
                    cell.classList.add("lightCell");
                }
                else {
                    cell.classList.add("darkCell");
                }

                if (lastMove !== null) {
                    if (lastMove.from.horizontalLocation === column &&
                        lastMove.from.verticalLocation === row) {
                        cell.classList.add("highlightPreviousFrom");
                    }
                    else if (lastMoveCapture !== null &&
                        lastMoveCapture.from.horizontalLocation === column &&
                        lastMoveCapture.from.verticalLocation === row) {
                        cell.classList.add("highlightCapture");
                    }
                    else if (lastMove.to.horizontalLocation === column &&
                        lastMove.to.verticalLocation === row) {
                        cell.classList.add("highlightPreviousTo");
                    }
                }
            }
        }
    }

    public makeMove(move: string, promotion: string) {
        console.log("Making move " + move + " with promotion " + promotion);
        this.board.makeMoveFromString(move);
        if (promotion.length > 0) {
            this.changePromotionFromString(promotion);
        }
    }

    public pieceSelected(id: string) {
        console.log("pieceSelected to " + id);

        if (this.waitingForConfirmation) {
            console.log("Waiting for confirmation");
            return;
        }

        if (this.game !== null && this.game.moves !== null &&
            this.game.moves.length !== this.currentMoveNumber) {
            console.log("Not in last move");
            return;
        }

        let rowIndex: number = parseInt(id[0]);
        let columnIndex: number = parseInt(id[2]);
        let identifier = rowIndex + "-" + columnIndex;

        if (this.previousAvailableMoves.length > 0) {

            let selectedMove: ChessMove | null = null;
            for (let i = 0; i < this.previousAvailableMoves.length; i++) {

                let move: ChessMove = this.previousAvailableMoves[i];
                let moveId: string = move.to.verticalLocation + "-" + move.to.horizontalLocation;
                let element = document.getElementById(moveId);

                if (element?.id === identifier) {
                    selectedMove = move;
                }

                element?.classList.remove("highlightMoveAvailable");
            }

            this.previousAvailableMoves = [];

            if (selectedMove !== null) {
                // Make selected move
                /*let locations: ChessMove[] =*/ this.board.makeMove(selectedMove, true);
                this.drawBoard();

                if (this.board.lastMovePromotion() !== null) {

                    let queenPromotionElement = document.getElementById("promotionRadioQueen") as HTMLInputElement;
                    queenPromotionElement.checked = true;

                    this.showPromotionDialog(true);
                }
                else {
                    this.setBoardStatus(0, 0);

                    this.showConfirmationDialog(true);
                }

                return;
            }
        }

        if (columnIndex >= ChessBoard.BOARD_SIZE ||
            rowIndex >= ChessBoard.BOARD_SIZE) {
            console.log("Only de-select the current selection");
            return;
        }

        let moves: ChessMove[] = this.board.getAvailableMoves(columnIndex, rowIndex);

        if (moves.length > 0) {
            for (let i = 0; i < moves.length; i++) {
                let move: ChessMove = moves[i];
                let element = document.getElementById(move.to.verticalLocation + "-" + move.to.horizontalLocation);
                element?.classList.add("highlightMoveAvailable");
                this.previousAvailableMoves[i] = move;
            }
        }
    }

    public confirm = (): void => {
        console.log("confirmed");
        this.showConfirmationDialog(false);
        this.showPromotionDialog(false);
    }

    public cancel = (): void => {
        console.log("cancel");
        this.showConfirmationDialog(false);
        this.showPromotionDialog(false);
        this.undo();
    }

    private showConfirmationDialog(show: boolean) {
        this.waitingForConfirmation = show;
        let confirmationDialogElement = document.getElementById("confirmation");
        if (confirmationDialogElement !== null) {
            confirmationDialogElement.style.display = show ? "inline" : "none";
        }
    }

    private showPromotionDialog(show: boolean) {
        this.waitingForConfirmation = show;
        let promotionDialogElement = document.getElementById("promotionDialog");
        if (promotionDialogElement !== null) {
            promotionDialogElement.style.display = show ? "inline" : "none";
        }
    }

    private changePromotionFromString(name: string): boolean {
        console.log("changePromotionFromString to " + name);
        if (name === "Queen") {
            // No changes to promotion
            return false;
        }
        else if (name === "Knight") {
            this.board.changePromotion(ChessPiece.Knight);
        }
        else if (name === "Rook") {
            this.board.changePromotion(ChessPiece.Rook);
        }
        else if (name === "Bishop") {
            this.board.changePromotion(ChessPiece.Bishop);
        }

        return true;
    }

    public changePromotion(name: string) {
        let promotionDialogElement = document.getElementById("promotionDialog");
        if (promotionDialogElement !== null) {
            promotionDialogElement.style.display = "none";

            let nodes: NodeList = document.getElementsByName(name)
            for (let i: number = 0; i < nodes.length; i++) {
                let radioElement = nodes[i] as HTMLInputElement;
                if (radioElement.checked === true) {
                    if (this.changePromotionFromString(radioElement.value)) {
                        this.drawBoard();
                    }
                    return;
                }
            }

        }
    }

    public undo() {
        let commentDialogElement = document.getElementById("commentDialog");
        if (commentDialogElement !== null) {
            commentDialogElement.style.display = "none";
        }

        this.board.undo();
        this.drawBoard();
    }

    public getLastMoveAsString(): string | undefined {
        return this.board.lastMove()?.getMoveString();
    }

    public getLastMovePromotionAsString(): string {
        let lastPromotion = this.board.lastMovePromotion();
        if (lastPromotion !== null) {
            return lastPromotion.piece.toString();
        }

        return "";
    }

    public setComment(commentText: string) {
        commentText = commentText !== null ? commentText : "";

        let commentElement = document.getElementById("LastComment") as HTMLDivElement;
        commentElement.innerText = commentText;
    }

    public setThinkTime(moveIndex: number, thinkTime: number) {
        let thinkTimeElement = document.getElementById("ThinkTime") as HTMLDivElement;

        let minutes = 0;
        let seconds = thinkTime;
        if (seconds > 60) {
            minutes = Math.floor(seconds / 60);
            seconds %= 60;
        }

        if (minutes > 0) {
            thinkTimeElement.innerText = "Move " + moveIndex + " think time was " + minutes + " minutes and " + seconds + " seconds.";
        }
        else {
            thinkTimeElement.innerText = "Move " + moveIndex + " think time was " + seconds + " seconds.";
        }
    }

    public setBoardStatus(currentMoveIndex: number, moves: number) {
        let statusElement = document.getElementById("status") as HTMLDivElement;
        let status = this.board.GetBoardState();
        let gameStatusMessage = "";

        console.log("currentMoveIndex: " + currentMoveIndex + ", moves: " + moves);
        if (currentMoveIndex !== moves) {
            gameStatusMessage = "Move ";
            gameStatusMessage += currentMoveIndex;
        }

        switch (status) {
            case ChessBoardState.StaleMate:
                gameStatusMessage = "Stalemate";
                break;

            case ChessBoardState.Check:
                gameStatusMessage = "Check => " + gameStatusMessage;
                break;

            case ChessBoardState.CheckMate:
                gameStatusMessage = "Checkmate!";
                break;

            default:
                break;
        }

        statusElement.innerText = gameStatusMessage;
    }

    private moveHistory(direction: number) {
        this.currentMoveNumber += direction;
        console.log("direction: " + direction);
        console.log("currentMoveNumber: " + this.currentMoveNumber);

        if (this.currentMoveNumber < 1) {
            this.currentMoveNumber = 1;
        }
        else if (this.currentMoveNumber > this.game.moves.length) {
            this.currentMoveNumber = this.game.moves.length;
        }
        this.makeNumberOfMoves(this.game, this.currentMoveNumber);
    }

    public firstMove() {
        console.log("to first move");
        this.moveHistory(-999999);
    }

    public previousMove() {
        console.log("previous move");
        this.moveHistory(-1);
    }

    public nextMove() {
        console.log("next move");
        this.moveHistory(1);
    }

    public lastMove() {
        console.log("to last move");
        this.moveHistory(999999);
    }

    public animateNextMove() {
        console.log("animating next move");
        this.moveHistory(1);

        if (this.game.moves.length !== this.currentMoveNumber) {
            setTimeout(() => {
                this.animateNextMove();
            }, 1000);
        }
    }
}
