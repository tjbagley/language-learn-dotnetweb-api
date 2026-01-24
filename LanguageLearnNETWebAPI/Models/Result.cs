namespace LanguageLearnNETWebAPI.Models
{
    public class Result<TData>
    {
        public Result(TData data)
        {
            Data = data;
        }

        public Result(int statusCode, string message)
        {
            ErrorStatusCode = statusCode;
            Message = message;
        }

        public TData? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        private int? ErrorStatusCode { get; set; } = null;

        public bool IsSuccess => ErrorStatusCode == null;
        public int StatusCode => ErrorStatusCode ?? StatusCodes.Status200OK;
    }
}
