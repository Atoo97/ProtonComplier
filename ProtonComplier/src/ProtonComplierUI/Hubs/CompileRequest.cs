namespace ProtonComplierUI.Hubs
{
    public class CompileRequest
    {
        public string Code { get; set; } = null!;
        public string ConnectionId { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public bool Lexical { get; set; }
        public bool Syntax { get; set; }
        public bool Semantical { get; set; }
    }

}
