using System.Net;

namespace MyChess.Interfaces
{
    public class HandlerError
    {
        public string Detail { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
        public int Status { get; set; } = (int)HttpStatusCode.InternalServerError;
        public string Title { get; set; } = string.Empty;
    }
}
