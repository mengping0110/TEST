using MimeDetective;

namespace GC.Common.Helper
{
    /// <summary>
    /// MimeDetective類別庫(檢查檔案格式是否正確)
    /// </summary>
    public static class MimeDetectiveHepler
    {
        private static readonly IContentInspector Inspector;

        static MimeDetectiveHepler()
        {
            Inspector = new ContentInspectorBuilder()
            {
                Definitions = MimeDetective.Definitions.DefaultDefinitions.All()
            }.Build();
        }

        public static ResultFileType GetFileType(this byte[] Input, bool ResetPosition = true)
        {
            Stream stream = new MemoryStream(Input);
            return GetFileType(stream, ResetPosition);
        }

        public static ResultFileType GetFileType(this Stream Input, bool ResetPosition = true)
        {
            var Content = ContentReader.Default.ReadFromStream(Input, ResetPosition);
            var Results = Inspector.Inspect(Content);

            if (!Results.Any()) return null;

            return new ResultFileType()
            {
                Extension = Results.ByFileExtension().First().Extension,
                Mime = Results.ByMimeType().First().MimeType
            };
        }
    }

    public class ResultFileType
    {
        public string Extension { get; internal set; } = string.Empty;
        public string Mime { get; internal set; } = string.Empty;
    }
}
