var MyChessPlay = MyChessPlay || {};
let _pieceSize = 40;
let _canvasElement;
let _context;
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
        };
        img.src = `/images/${file}.svg`;
        _images[file] = img;
    }
};
loadImages();
const resizeCanvas = () => {
    if (_canvasElement !== undefined) {
        const element = document.getElementById("game");
        const maxWidth = document.documentElement.clientWidth;
        const maxHeight = document.documentElement.clientHeight;
        _canvasElement.width = _pieceSize * 8;
        _canvasElement.height = _pieceSize * 8;
        const aspectRatio = _canvasElement.width / _canvasElement.height;
        const availableWidth = maxWidth * 0.9;
        const availableHeight = maxHeight * 0.9;
        let resizeWidth = availableWidth;
        let resizeHeight = availableWidth / aspectRatio;
        if (availableHeight < resizeHeight) {
            console.log(`Height resized to ${resizeHeight} but space only for ${availableHeight}`);
            resizeHeight = availableHeight;
            resizeWidth = availableHeight * aspectRatio;
            if (availableWidth < resizeWidth) {
                console.log(`Width resized to ${resizeWidth} but space only for ${availableWidth}`);
            }
        }
        element.style.width = `${Math.round(resizeWidth)}px`;
        element.style.height = `${Math.round(resizeHeight)}px`;
        const marginTop = (maxHeight - resizeHeight) / 2;
        const marginLeft = (maxWidth - resizeWidth) / 2;
        element.style.marginTop = `${Math.round(marginTop)}px`;
        element.style.marginLeft = `${Math.round(marginLeft)}px`;
    }
};
window.addEventListener('resize', () => {
    console.log("resize");
    resizeCanvas();
});
const drawImage = (image, row, column) => {
    const x = Math.floor(column * _pieceSize);
    const y = Math.floor(row * _pieceSize);
    const img = _images[image];
    _context.drawImage(img, x, y);
};
const setTouchHandlers = (canvas) => {
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
MyChessPlay.draw = (board) => {
    console.log(board);
    if (_context === undefined || _imagesLoaded !== _imagesToLoad || board === undefined) {
        console.log("Not yet ready to draw");
        return;
    }
    _context.save();
    _context.imageSmoothingEnabled = true;
    _context.fillStyle = "gray";
    _context.fillRect(0, 0, _canvasElement.width, _canvasElement.height);
    _context.fill();
    for (let row = 0; row < board.length; row++) {
        const r = board[row];
        for (let column = 0; column < r.length; column++) {
            const item = r[column];
            drawImage(item.image, row, column);
        }
    }
    _context.restore();
};
//# sourceMappingURL=play.js.map