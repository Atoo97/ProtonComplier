namespace ProtonComplierUI.Hubs
{
    public class CompileRequest
    {
        public IFormFile File { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Result { get; set; } = null!;
        public string ConnectionId { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public bool Lexical { get; set; }
        public bool Syntax { get; set; }
        public bool Semantical { get; set; }
    }

}
