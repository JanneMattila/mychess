using MyChess.Interfaces;

namespace MyChess.Client.Extensions;

public static class MyChessGameExtensions
{
    public static string GetDate(this MyChessGame game)
    {
        ArgumentNullException.ThrowIfNull(game);
        if (game.Moves.Count > 0)
        {
            var move = game.Moves[^1];
            var now = DateTimeOffset.UtcNow;
            var update = move.End;

            var ts = (now - update);

            if (ts.TotalSeconds < 60)
            {
                return "Now";
            }

            var minutes = Math.Floor(ts.TotalSeconds / 60);
            var hours = Math.Floor(minutes / 60);
            var days = Math.Floor(hours / 24);
            if (days > 30)
            {
                return "Over month ago";
            }
            else if (days > 0)
            {
                var ext = days > 1 ? "s" : "";
                return $"{days} day{ext} ago";
            }
            else if (hours > 0)
            {
                var ext = hours > 1 ? "s" : "";
                return $"{hours} hour{ext} ago";
            }
            else if (minutes > 0)
            {
                var ext = minutes > 1 ? "s" : "";
                return $"{minutes} minute{ext} ago";
            }
            else
            {
                return "Now";
            }
        }
        return "";
    }

    public static string GetStatus(this MyChessGame game)
    {
        ArgumentNullException.ThrowIfNull(game);
        if (!string.IsNullOrEmpty(game.State) && game.State != "Normal" &&
            !string.IsNullOrEmpty(game.StateText))
        {
            return game.StateText;
        }
        return string.Empty;
    }

    public static string GetOpponent(this MyChessGame game)
    {
        ArgumentNullException.ThrowIfNull(game);
        return string.Empty;
    }
}
