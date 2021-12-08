namespace VideoDown.Model
{
    public class HttpReturnData
    {
        public bool Success { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public string Extras { get; set; }
        public string Timestamp { get; set; }
    }
}