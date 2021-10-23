var MyChessPlay = MyChessPlay || {};
MyChessPlay.pieceSelected = function (evt) {
    console.log(evt);
    //evt.preventDefault();
};
MyChessPlay.draw = function (board) {
    console.log(board);
    var element = document.getElementById("table-game");
    if (element == null || element == undefined) {
        console.log("Not yet ready to draw");
        return;
    }
    var pieceSize = 40;
    var table = "";
    for (var row = 0; row < board.length; row++) {
        var r = board[row];
        var html = "";
        for (var column = 0; column < r.length; column++) {
            var item = r[column];
            var cell = "<td id='" + item.key + "' width='" + pieceSize + "px' height='" + pieceSize + "px' class='" + item.background + "' onclick='MyChessPlay.pieceSelected'>";
            cell += "<img src='" + item.image + "' width='" + pieceSize + "px' height='" + pieceSize + "px' />";
            cell += "</td>";
            html += cell;
        }
        table += "<tr>" + html + "</tr>";
    }
    element.innerHTML = table;
};
//# sourceMappingURL=play.js.map