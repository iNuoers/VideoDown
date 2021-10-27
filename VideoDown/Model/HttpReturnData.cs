namespace VideoDown.Model
{
    public class HttpReturnData
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public object data { get; set; }
        public string extras { get; set; }
        public string timestamp { get; set; }

    }
}
