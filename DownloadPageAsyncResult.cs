using System.Net;
using System.Net.Http.Headers;

namespace BitcoinRichList
{
    public class DownloadPageAsyncResult
    {
        public string? Result { get; set; }
        public string? ReasonPhrase { get; set; }
        public HttpResponseHeaders? Headers { get; set; }
        public HttpStatusCode Code { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
