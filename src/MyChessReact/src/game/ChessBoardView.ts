import { ChessBoard } from "./ChessBoard";
import { ChessMove } from "./ChessMove";
import { ChessBoardPiece } from "./ChessBoardPiece";
import { ChessPiece } from "./ChessPiece";
import { ChessBoardState } from "./ChessBoardState";
import { MyChessGame } from "../models/MyChessGame";
import { ChessPlayer } from "./ChessPlayer";
import { setTimeout } from "timers";
import { QueryStringParser } from "../helpers/QueryStringParser";
import { MyChessGameMove } from "../models/MyChessGameMove";
import { Database, DatabaseFields } from "../data/Database";
import { GameStateFilter } from "../models/GameStateFilter";
import { getAppInsights } from "../components/TelemetryService";

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

    private isLocalGame: boolean = true;
    private isNewGame: boolean = false;
    private isDialogOpen: boolean = false;
    private friendID: string = "";

    private pieceSize: number = 45;
    private start: string = "";
    private endpoint: string = "";
    private accessToken: string = "";
    private me: string = "";

    private click = this.clickHandler.bind(this);
    private keyup = this.keyupHandler.bind(this);
    private touchstart = this.touchstartHandler.bind(this);
    private touchend = this.touchendHandler.bind(this);
    private resize = this.resizeHandler.bind(this);

    private ai = getAppInsights();

    public initialize() {
        // Start preparing the board
        this.game = new MyChessGame();
        this.board = new ChessBoard();
        this.board.initialize();
        this.previousAvailableMoves = [];

        this.setBoardStatus(0, 0);
        this.setComment("");
        this.setThinkTime(0, -1);

        // Update game board to the screen
        this.drawBoard();

        this.resizeHandler();
    }

    private clickHandler(event: MouseEvent) {
        if (!event.defaultPrevented) {
            // Add "de-selection" when clicking outside the board
            this.pieceSelected("9-9");
        }
    }

    private touchstartHandler(event: TouchEvent) {
        this.touch = undefined;
        if (event.changedTouches.length === 1) {
            this.touch = event.changedTouches[0];
        }
    }

    private touchendHandler(event: TouchEvent) {
        if (this.touch !== undefined &&
            event.changedTouches.length === 1) {
            // const delta = 40;
            // let touchEnd = event.changedTouches[0];
            // if (Math.abs(touchEnd.clientY - this.touch.clientY) > delta) {
            //     if (touchEnd.clientY < this.touch.clientY - delta) {
            //         this.firstMove();
            //     }
            //     else {
            //         this.lastMove();
            //     }
            // }
            // else if (Math.abs(touchEnd.clientX - this.touch.clientX) > delta) {
            //     if (touchEnd.clientX < this.touch.clientX - delta) {
            //         this.previousMove();
            //     }
            //     else {
            //         this.nextMove();
            //     }
            // }
        }
    }

    private keyupHandler(event: KeyboardEvent) {

        if (this.isDialogOpen) {
            return;
        }

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
        event.preventDefault();
    }

    public addEventHandlers() {
        document.addEventListener("click", this.click);
        document.addEventListener('keyup', this.keyup);
        document.addEventListener('touchstart', this.touchstart);
        document.addEventListener('touchend', this.touchend);
        window.addEventListener('resize', this.resize);
    }

    public removeEventHandlers() {
        document.removeEventListener("click", this.click);
        document.removeEventListener('keyup', this.keyup);
        document.removeEventListener('touchstart', this.touchstart);
        document.removeEventListener('touchend', this.touchend);
        window.removeEventListener('resize', this.resize);;
    }

    public resizeHandler() {
        const table = document.getElementById("table-game") as HTMLTableElement;
        if (table) {
            const width = Math.floor(window.innerWidth * 0.95);
            const height = Math.floor(window.innerHeight * 0.75);
            const size = Math.min(width, height);
            console.log("" + width + "x" + height + " => " + size);
            table.style.width = size + "px";
            table.style.height = size + "px";
            this.pieceSize = Math.floor(size / 8);
            this.drawBoard();
        }
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
            img.className = "img";
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

    public async load(endpoint: string, accessToken?: string, me?: string) {
        // Determine if this game is:
        // - local game
        // - new game *WITH* friendID in url
        // - existing game
        const path = window.location.pathname;
        const query = window.location.search;
        const queryString = QueryStringParser.parse(query);

        this.endpoint = endpoint;
        if (accessToken) {
            this.accessToken = accessToken;
        }

        this.initialize();

        this.start = new Date().toISOString();

        if (path.indexOf("/local") !== -1) {
            console.log("local game");
            this.isLocalGame = true;

            this.ai.trackEvent({
                name: "Play-NewGame", properties: {
                    isLocalGame: true,
                }
            });

            this.game = new MyChessGame();

            const json = Database.get<string>(DatabaseFields.GAMES_LOCAL_GAME_STATE);
            if (json) {
                // Try to load game state from previously stored state
                try {
                    this.game = JSON.parse(json) as MyChessGame;
                    console.log(this.game);

                    this.currentMoveNumber = this.makeNumberOfMoves(this.game, this.game.moves.length);
                } catch (error) {
                    console.log(error);
                    this.ai.trackException({ exception: error });

                    this.game = new MyChessGame();
                }
            }
        }
        else {
            this.isLocalGame = false;
            if (me) {
                this.me = me;
            }

            if (path.indexOf("/new") !== -1) {

                this.ai.trackEvent({
                    name: "Play-NewGame", properties: {
                        isLocalGame: false,
                    }
                });

                console.log("new game");
                this.isNewGame = true;
                const friendIDParameter = queryString.get("friendID");
                if (friendIDParameter) {
                    this.friendID = friendIDParameter;
                }
                else {
                    throw new Error("Required parameter for friend is missing!");
                }
            }
            else {
                console.log("existing game");
                if (!accessToken) {
                    console.log("skip fetching game before accessToken is available")
                }
                else {
                    const gameID = path.substring(path.lastIndexOf("/") + 1);
                    const state = queryString.get("state") ?? "";

                    try {
                        const request: RequestInit = {
                            method: "GET",
                            headers: {
                                "Accept": "application/json",
                                "Authorization": "Bearer " + this.accessToken
                            }
                        };
                        const response = await fetch(this.endpoint + `/api/games/${gameID}?state=${state}`, request);
                        this.game = await response.json() as MyChessGame;
                        console.log(this.game);

                        // let animatedMoves = Math.min(3, this.game.moves.length);
                        // let moves = this.game.moves.length - animatedMoves;
                        this.currentMoveNumber = this.makeNumberOfMoves(this.game, this.game.moves.length);
                        // setTimeout(() => {
                        //     this.animateNextMove();
                        // }, 1000);
                    }
                    catch (error) {
                        console.log(error);
                        this.ai.trackException({ exception: error });

                        // $("#errorText").text(textStatus);
                        // $("#errorDialog").dialog();
                    }
                }
            }
        }

        this.loadImages();
    }

    public async postNewGame(game: MyChessGame) {
        const request: RequestInit = {
            method: "POST",
            body: JSON.stringify(game),
            headers: {
                "Accept": "application/json",
                "Authorization": "Bearer " + this.accessToken
            }
        };

        try {
            const response = await fetch(this.endpoint + "/api/games", request);
            this.game = await response.json() as MyChessGame;
            console.log(this.game);

            document.location.href = `/play/${this.game.id}?state=${GameStateFilter.WAITING_FOR_OPPONENT}`;
        } catch (error) {
            console.log(error);
            this.ai.trackException({ exception: error });

            const errorMessage = error.errorMessage ? error.errorMessage : "Unable to create new game.";
            this.showError(errorMessage);
            this.undo();
        }
    }

    public async postMove(move: MyChessGameMove) {
        const request: RequestInit = {
            method: "POST",
            body: JSON.stringify(move),
            headers: {
                "Accept": "application/json",
                "Authorization": "Bearer " + this.accessToken
            }
        };

        try {
            const response = await fetch(this.endpoint + `/api/games/${this.game.id}/moves`, request);
            if (response.ok) {
                console.log("Move submitted successfully");
                this.game.moves.push(move);
                this.currentMoveNumber++;
                this.makeNumberOfMoves(this.game, this.game.moves.length);
            }
            else {
                throw new Error("Could not make move submit!")
            }
        } catch (error) {
            console.log(error);
            this.ai.trackException({ exception: error });

            const errorMessage = error.errorMessage ? error.errorMessage : "Unable to post your move.";
            this.showError(errorMessage);
            this.undo();
        }
    }

    private makeNumberOfMoves(game: MyChessGame, movesCount: number): number {
        this.initialize();
        this.game = game;

        let count = Math.min(game.moves.length, movesCount);
        console.log("going to make " + count + " moves");

        if (count > 0) {
            for (let i = 0; i < count; i++) {
                let move = game.moves[i];
                let promotion = move.promotion !== null ? move.promotion : "";
                this.makeMove(move.move, promotion);
            }
            this.setBoardStatus(count, game.moves.length);

            let move = game.moves[count - 1];

            const start = Date.parse(move.start);
            const end = Date.parse(move.end);

            this.setThinkTime(count, end - start);
            this.setComment(move.comment);
        }
        else {
            this.setThinkTime(0, -1);
            this.setComment("");
        }
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
                cell.width = this.pieceSize + "px";
                cell.height = this.pieceSize + "px";
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
        if (promotion !== undefined && promotion.length > 0) {
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
            console.log(`Not in last move: ${this.game.moves.length} <> ${this.currentMoveNumber}`);
            return;
        }

        if (!this.isLocalGame && !this.isNewGame) {
            if (this.board.currentPlayer === ChessPlayer.White &&
                this.game.players.white.id !== this.me) {
                console.log(`Not current players turn. Player is ${this.me} and turn is on player ${this.game.players.white.id}`);
                return;
            }
            else if (this.board.currentPlayer === ChessPlayer.Black &&
                this.game.players.black.id !== this.me) {
                console.log(`Not current players turn. Player is ${this.me} and turn is on player ${this.game.players.black.id}`);
                return;
            }
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

                    this.showError("");
                    this.showPromotionDialog(true);
                }
                else {
                    this.setBoardStatus(0, 0);

                    this.showError("");
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

    public confirmMove = (): void => {
        console.log("move confirmed");
        this.showError("");
        this.showConfirmationDialog(false);
        this.showPromotionDialog(false);

        if (this.isLocalGame) {
            const lastMove = this.getLastMoveAsString();
            const lastPromotion = this.getLastMovePromotionAsString();
            if (!lastMove) {
                console.log("no last move available");
                return;
            }

            const move = new MyChessGameMove()
            move.move = lastMove;
            move.promotion = lastPromotion;
            move.comment = "";
            move.start = this.start;
            move.end = new Date().toISOString();

            this.game.moves.push(move);
            this.currentMoveNumber++;

            Database.set(DatabaseFields.GAMES_LOCAL_GAME_STATE, JSON.stringify(this.game));
            this.start = new Date().toISOString();
        }
        this.showCommentDialog(!this.isLocalGame);
        this.showGameNameDialog(!this.isLocalGame && this.isNewGame);
        this.isDialogOpen = !this.isLocalGame;
    }

    public confirmComment = (): void => {
        console.log("comment confirmed");

        const commentElement = document.getElementById("comment") as HTMLTextAreaElement;
        const content = commentElement?.value;
        const comment = content ? content : "";

        const lastMove = this.getLastMoveAsString();
        const lastPromotion = this.getLastMovePromotionAsString();
        if (!lastMove) {
            console.log("no last move available");

            this.ai.trackEvent({
                name: "Play-Errors", properties: {
                    type: "NoLastMoveAvailable",
                }
            });

            return;
        }

        const move = new MyChessGameMove()
        move.move = lastMove;
        move.promotion = lastPromotion;
        move.comment = comment;
        move.start = this.start;
        move.end = new Date().toISOString();

        console.log(this.isNewGame);
        if (this.isNewGame) {
            const gameNameElement = document.getElementById("gameName") as HTMLInputElement;
            const gameName = gameNameElement?.value;
            if (!gameName) {
                console.log("no mandatory game name provided");

                this.ai.trackEvent({
                    name: "Play-Errors", properties: {
                        type: "NoGameNameProvided",
                    }
                });

                return;
            }

            const game = new MyChessGame()
            game.players.black.id = this.friendID;
            game.name = gameName;
            game.moves.push(move)

            this.postNewGame(game);
        }
        else {
            this.postMove(move);
        }

        this.showGameNameDialog(false);
        this.showCommentDialog(false);
        this.isDialogOpen = false;
    }

    public resignGame = (): void => {
        console.log("game resigned");


        this.ai.trackEvent({
            name: "Play-Resign"
        });

        if (this.isLocalGame) {
            Database.delete(DatabaseFields.GAMES_LOCAL_GAME_STATE);
            this.game = new MyChessGame();
            this.board = new ChessBoard();
            this.board.initialize();
            this.previousAvailableMoves = [];
            this.currentMoveNumber = 0;

            this.drawBoard();
        }
    }

    public cancel = (): void => {
        console.log("cancel");

        this.ai.trackEvent({
            name: "Play-Cancel"
        });

        this.showError("");
        this.showConfirmationDialog(false);
        this.showPromotionDialog(false);
        this.undo();
    }

    private showConfirmationDialog(show: boolean) {
        this.waitingForConfirmation = show;
        let confirmationDialogElement = document.getElementById("confirmation");
        if (confirmationDialogElement !== null) {
            confirmationDialogElement.style.display = show ? "inline" : "none";
            this.isDialogOpen = true;
        }
    }

    private showPromotionDialog(show: boolean) {
        this.waitingForConfirmation = show;
        let promotionDialogElement = document.getElementById("promotionDialog");
        if (promotionDialogElement !== null) {
            promotionDialogElement.style.display = show ? "inline" : "none";
        }
    }

    private showGameNameDialog(show: boolean) {
        this.waitingForConfirmation = show;
        let gameNameDialogElement = document.getElementById("gameNameDialog");
        if (gameNameDialogElement !== null) {
            gameNameDialogElement.style.display = show ? "inline" : "none";
            if (show) {
                gameNameDialogElement.scrollIntoView();
                gameNameDialogElement.focus();
            }
        }
    }

    private showError(message: string) {
        let element = document.getElementById("error");
        if (element !== null) {
            const show = message.length > 0;
            element.style.display = show ? "inline" : "none";
            if (show) {
                element.scrollIntoView();
                element.focus();
            }
        }
    }

    private showCommentDialog(show: boolean) {
        this.waitingForConfirmation = show;
        let commentDialogElement = document.getElementById("commentDialog");
        if (commentDialogElement !== null) {
            commentDialogElement.style.display = show ? "inline" : "none";
            if (show) {
                commentDialogElement.scrollIntoView();
                commentDialogElement.focus();
            }
        }
    }

    private changePromotionFromString(name: string): boolean {
        console.log("changePromotionFromString to " + name);

        this.ai.trackEvent({
            name: "Play-Promotion", properties: {
                type: name,
            }
        });

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
        commentText = commentText !== null ? commentText : "&nbsp;";

        let commentElement = document.getElementById("LastComment") as HTMLDivElement;
        commentElement.innerHTML = commentText;
    }

    public setThinkTime(moveIndex: number, thinkTime: number) {
        let thinkTimeElement = document.getElementById("ThinkTime") as HTMLDivElement;

        if (thinkTime === -1) {
            thinkTimeElement.innerText = "";
            return;
        }

        let minutes = 0;
        let seconds = thinkTime / 1000;
        if (seconds > 60) {
            minutes = Math.floor(seconds / 60);
            seconds = Math.floor(seconds % 60);
        }
        else {
            seconds = Math.floor(seconds);
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
                gameStatusMessage = "Check " + gameStatusMessage;
                break;

            case ChessBoardState.CheckMate:
                gameStatusMessage = "Checkmate!";
                break;

            default:
                gameStatusMessage += "&nbsp;"
                break;
        }

        statusElement.innerHTML = gameStatusMessage;
    }

    private moveHistory(direction: number) {
        this.currentMoveNumber += direction;
        console.log("direction: " + direction);
        console.log(this.game);
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
        this.ai.trackEvent({
            name: "Play-MoveHistory", properties: {
                type: "First",
            }
        });

        this.moveHistory(-999999);
    }

    public previousMove() {
        console.log("previous move");
        this.ai.trackEvent({
            name: "Play-MoveHistory", properties: {
                type: "Previous",
            }
        });

        this.moveHistory(-1);
    }

    public nextMove() {
        console.log("next move");
        this.ai.trackEvent({
            name: "Play-MoveHistory", properties: {
                type: "Next",
            }
        });

        this.moveHistory(1);
    }

    public lastMove() {
        console.log("to last move");
        this.ai.trackEvent({
            name: "Play-MoveHistory", properties: {
                type: "Last",
            }
        });

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
