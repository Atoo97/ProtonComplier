namespace ProtonComplierUI.Hubs
{
    public class CompileRequest
    {
        public string ConnectionId { get; set; } = null!;
        public IFormFile File { get; set; } = null!;

        public string FileName
        {
            get => _fileName;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _fileName = Path.GetFileNameWithoutExtension(value); // removes ".prtn" or any extension
                }
                else
                {
                    _fileName = string.Empty;
                }
            }
        }

        public string Code { get; set; } = null!;
        public string Result { get; set; } = null!;
        public string Console { get; set; } = null!;
        public bool Lexical { get; set; }
        public bool Syntax { get; set; }
        public bool Semantical { get; set; }

        private string _fileName = string.Empty;
    }
}
