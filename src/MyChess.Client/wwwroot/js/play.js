var MyChessPlay = MyChessPlay || {};
let _animationRunning = false;
let _animationUpdate = 0;
let _pieceSizeMin = 20;
let _pieceSize = _pieceSizeMin;
let _canvasElement;
let _context;
let _game;
let _dotnetRef;
let _imagesLoaded = 0;
let _imagesToLoad = -1;
const _images = new Object();
const loadImages = () => {
    const files = [
        "bishop_black",
        "bishop_white",
        "empty",
        "king_black",
        "king_white",
        "knight_black",
        "knight_white",
        "pawn_black",
        "pawn_white",
        "queen_black",
        "queen_white",
        "rook_black",
        "rook_white"
    ];
    _imagesToLoad = files.length;
    for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const img = new Image();
        img.onload = function () {
            _imagesLoaded++;
            if (_imagesLoaded === _imagesToLoad) {
                MyChessPlay.draw(_game);
            }
        };
        img.src = `/images/${file}.svg`;
        _images[file] = img;
    }
};
loadImages();
const resizeCanvas = () => {
    if (_canvasElement) {
        const element = document.getElementById("game");
        const maxWidth = document.documentElement.clientWidth;
        const maxHeight = document.documentElement.clientHeight;
        const availableWidth = maxWidth * 0.98;
        const availableHeight = maxHeight * 0.70;
        let size = availableWidth;
        if (availableHeight < availableWidth) {
            size = availableHeight;
        }
        const ratio = window.devicePixelRatio;
        _pieceSize = Math.max(Math.floor(size / 8), _pieceSizeMin);
        if (_dotnetRef !== undefined) {
            _dotnetRef.invokeMethod("UpdateSize", _pieceSize);
        }
        const newSize = _pieceSize * 8;
        _canvasElement.style.width = `${Math.round(newSize)}px`;
        _canvasElement.style.height = `${Math.round(newSize)}px`;
        _canvasElement.width = newSize * ratio;
        _canvasElement.height = newSize * ratio;
        MyChessPlay.draw(_game);
    }
};
window.addEventListener('resize', () => {
    console.log("resize");
    resizeCanvas();
});
window.addEventListener('keydown', (event) => {
    if (_animationRunning)
        return;
    const code = event.code;
    console.log(`keyup: ${event.code}`);
    if (_dotnetRef !== undefined) {
        // Minimal impact to any keyboard activity
        switch (code) {
            case "Home":
            case "ArrowLeft":
            case "ArrowDown":
            case "PageDown":
            case "ArrowRight":
            case "ArrowUp":
            case "PageUp":
            case "End":
                _dotnetRef.invokeMethodAsync("CanvasOnKeyDown", event.code)
                    .then((cancel) => {
                    console.log(`cancel: ${cancel}`);
                    if (cancel) {
                        event.preventDefault();
                        event.stopPropagation();
                        event.cancelBubble = true;
                    }
                });
                break;
            default:
                break;
        }
    }
});
const setTouchHandlers = (canvas) => {
    //canvas.addEventListener('click', (event: MouseEvent) => {
    //    event.preventDefault();
    //    let x = Math.floor(event.offsetX / _pieceSize);
    //    let y = Math.floor(event.offsetY / _pieceSize);
    //    if (_dotnetRef !== undefined) {
    //        _dotnetRef.invokeMethod("CanvasOnClick", x, y);
    //    }
    //}, false);
};
MyChessPlay.initialize = (canvasElement, dotnetRef) => {
    console.log("=> initialize");
    _canvasElement = canvasElement;
    setTouchHandlers(_canvasElement);
    _dotnetRef = dotnetRef;
    _context = _canvasElement.getContext("2d");
    resizeCanvas();
    MyChessPlay.draw(undefined);
};
MyChessPlay.scrollToComment = () => {
    setTimeout(() => {
        const elementName = document.getElementById("gameName");
        const elementComment = document.getElementById("comment");
        if (elementName) {
            elementName.focus();
        }
        else if (elementComment) {
            elementComment.focus();
        }
        if (elementComment) {
            elementComment.scrollIntoView();
        }
    }, 100);
};
MyChessPlay.draw = (game, delta = 0) => {
    _game = game;
    if (_context === undefined || _imagesLoaded !== _imagesToLoad || game === undefined) {
        console.log("Not yet ready to draw");
        return;
    }
    console.log("draw");
    console.log(game);
    _context.save();
    const scale = window.devicePixelRatio;
    _context.scale(scale, scale);
    // Draw chess board
    for (let row = 0; row < 8; row++) {
        for (let column = 0; column < 8; column++) {
            const x = Math.floor(column * _pieceSize);
            const y = Math.floor(row * _pieceSize);
            _context.save();
            if ((row + column) % 2 == 0) {
                _context.fillStyle = "#A9A9A9";
            }
            else {
                _context.fillStyle = "#FFFFFF";
            }
            _context.fillRect(x, y, _pieceSize, _pieceSize);
            _context.fill();
            _context.restore();
        }
    }
    if (game.animations.length == 0) {
        for (const highlights of game.highlights) {
            const column = Math.floor(highlights.column * _pieceSize);
            const row = Math.floor(highlights.row * _pieceSize);
            _context.save();
            if (highlights.data === "HighlightPreviousFrom") {
                _context.fillStyle = "rgba(195, 140, 140, 0.9)";
            }
            else if (highlights.data === "HighlightCapture") {
                _context.fillStyle = "rgba(255, 216, 0, 0.9)";
            }
            else if (highlights.data === "HighlightPreviousTo") {
                _context.fillStyle = "rgba(140, 195, 140, 0.9)";
            }
            _context.fillRect(column, row, _pieceSize, _pieceSize);
            _context.fill();
            _context.restore();
        }
        for (const availableMove of game.availableMoves) {
            const column = Math.floor(availableMove.column * _pieceSize);
            const row = Math.floor(availableMove.row * _pieceSize);
            _context.save();
            _context.fillStyle = "rgba(0, 255, 0, 0.5)";
            _context.fillRect(column, row, _pieceSize, _pieceSize);
            _context.fill();
            _context.restore();
        }
    }
    for (const piece of game.pieces) {
        const column = Math.floor(piece.column * _pieceSize);
        const row = Math.floor(piece.row * _pieceSize);
        const img = _images[piece.data];
        _context.drawImage(img, column, row, _pieceSize, _pieceSize);
    }
    if (game.animations.length !== 0) {
        for (const animations of game.animations) {
            const dc = animations.to.column - animations.from.column;
            const dr = animations.to.row - animations.from.row;
            const column = animations.from.column + delta * dc;
            const row = animations.from.row + delta * dr;
            const x = Math.floor(column * _pieceSize);
            const y = Math.floor(row * _pieceSize);
            const img = _images[animations.data];
            _context.drawImage(img, x, y, _pieceSize, _pieceSize);
        }
    }
    // devicePixelRatio debugging helpers:
    //let { width: cssWidth, height: cssHeight } = _canvasElement.getBoundingClientRect();
    //console.log("Draw");
    //_context.font = "30px Arial";
    //_context.fillText( `dpr: ${window.devicePixelRatio}`, 10, 200);
    //_context.fillText(`canvas: ${_canvasElement.width}x${_canvasElement.height}`, 10, 230);
    //_context.fillText(`canvas.style: ${cssWidth}x${cssHeight}`, 10, 260);
    _context.restore();
};
const update = (timestamp) => {
    const delta = Math.min((timestamp - _animationUpdate) / 1000, 1.0);
    if (_context === undefined || _imagesLoaded !== _imagesToLoad || _game === undefined) {
        console.log("Not yet ready to draw animation");
        return;
    }
    console.log("Delta:", delta);
    MyChessPlay.draw(_game, delta);
    return delta;
};
const playAnimationFrame = (timestamp) => {
    if (_animationUpdate === 0) {
        _animationUpdate = timestamp;
    }
    const delta = update(timestamp);
    if (delta < 1 /* _animations !== undefined */) {
        window.requestAnimationFrame(playAnimationFrame);
    }
    else {
        // Animation has ended.
        console.log("AnimationEnded");
        _animationRunning = false;
        _dotnetRef.invokeMethodAsync("AnimationEnded");
    }
};
MyChessPlay.animate = (game) => {
    console.log("AnimationStarted");
    _game = game;
    _animationRunning = true;
    _animationUpdate = 0;
    playAnimationFrame(0);
};
//# sourceMappingURL=play.js.map