
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Tokens;

namespace GC.Common.Helper
{
    /// <summary>
    /// 檔案檢核
    /// </summary>
    public class FileDataCheckHelper
    {
        //1MB多少BYTES
        private const long BytesInMegabyte = 1024 * 1024;

        /// <summary>
        /// 檔案類型
        /// </summary>
        public enum FileTypeEnum
        {
            jpg,
            jpeg,
            png,
            pdf,
            odt,
            ods,
            gif,
            webp,
            doc,
            docx,
            xls,
            xlsx,
            mp4
        }

        /// <summary>
        /// 取得MIME
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".csv" => "text/csv",
                _ => "application/octet-stream",
            };
        }

        /// <summary>
        /// CheckFile 檢核檔案格式，並拋出真實的檔案格式
        /// </summary>
        /// <param name="fup"></param>
        /// <param name="allowMaxSize">單位MB</param>
        /// <param name="msg"></param>
        /// <param name="fileExtension"></param>
        /// <param name="fileTypes"></param>
        /// <returns></returns>
        public static bool CheckFile(Stream fileStream, long allowMaxSize, ref string msg, ref string fileExtension, params FileTypeEnum[] fileTypes)
        {
            if (fileStream == null || fileStream.Length <= 0)
            {
                msg = "請選擇上傳檔案";
                return false;
            }

            // 檢查檔案大小是否超過限制
            if (fileStream.Length >= allowMaxSize * BytesInMegabyte)
            {
                msg = $"傳入檔案大小超出允許值 {allowMaxSize}MB";
                return false;
            }

            try
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                ResultFileType rf = MimeDetectiveHepler.GetFileType(fileStream); // 使用 MimeDetective 檢查文件格式
                fileStream.Seek(0, SeekOrigin.Begin); // 重置流位置，方便後續操作

                if (rf != null)
                    fileExtension = rf.Extension;

                // 驗證副檔名是否在允許範圍
                if (Enum.TryParse(fileExtension.ToLower(), out FileTypeEnum _fileExtension))
                {
                    if (!fileTypes.Contains(_fileExtension))
                    {
                        msg = $"請上傳檔案格式為 {string.Join("、", fileTypes)} 的檔案";
                        return false;
                    }

                    //PDF安全性掃描
                    if (_fileExtension == FileTypeEnum.pdf) {
                        if (ScanPdfHasScript(fileStream))
                        {
                            msg = $"檔案內容有問題，請重新上傳";
                            return false;
                        }
                    }
                }
                else
                {
                    msg = $"未知的檔案格式: {fileExtension}";
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = $"檢查檔案時發生錯誤: {ex.Message}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// PDF安全性掃描 (使用傳入的 Stream)
        /// </summary>
        /// <param name="stream">PDF 檔案的資料流，呼叫端負責管理 Stream 的生命週期</param>
        /// <returns>若發現 JavaScript 或可疑動作則回傳 true</returns>
        public static bool ScanPdfHasScript(Stream stream)
        {
            if (stream == null)
            {
                return false;
            }

            long? originalPosition = null;
            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Seek(0, SeekOrigin.Begin);
            }

            try
            {
                using PdfDocument document = PdfDocument.Open(stream);
                // Check document-level JavaScript
                if (CheckDocumentLevelJavaScript(document)) return true;

                // Check OpenAction
                if (CheckOpenAction(document)) return true;

                // Check each page
                return document.GetPages().Any(p => CheckPageActions(p) || CheckPageAnnotations(p));
            }
            finally
            {
                // Restore stream position if possible
                if (originalPosition.HasValue && stream.CanSeek)
                {
                    stream.Seek(originalPosition.Value, SeekOrigin.Begin);
                }
            }
        }

        /// <summary>
        /// 檢查 PDF 文件層級是否包含 JavaScript 定義 (Names -> JavaScript)
        /// </summary>
        /// <param name="document">要檢查的 PdfDocument 物件</param>
        /// <returns>若在文件層級發現 JavaScript 定義則回傳 true，否則回傳 false</returns>
        private static bool CheckDocumentLevelJavaScript(PdfDocument document)
        {
            var catalog = document.Structure.Catalog.CatalogDictionary;
            return
                catalog.TryGet(NameToken.Names, out var names) &&
                names is DictionaryToken namesDict && namesDict.TryGet(NameToken.JavaScript, out var js);
        }

        /// <summary>
        /// 檢查 PDF Catalog 是否定義了 OpenAction（文件開啟時要執行的動作）
        /// </summary>
        /// <param name="document">要檢查的 PdfDocument 物件</param>
        /// <returns>若定義 OpenAction 則回傳 true，否則回傳 false</returns>
        private static bool CheckOpenAction(PdfDocument document)
        {
            var catalog = document.Structure.Catalog.CatalogDictionary;
            return catalog.TryGet(NameToken.OpenAction, out var openAction);
        }

        /// <summary>
        /// 檢查頁面是否定義了動作 (例如 /AA 或 /A)
        /// </summary>
        /// <param name="page">要檢查的 Page 物件</param>
        /// <returns>若頁面包含動作鍵則回傳 true，否則回傳 false</returns>
        private static bool CheckPageActions(Page page)
        {
            var dict = page.Dictionary;
            return dict.ContainsKey(NameToken.Aa) || dict.ContainsKey(NameToken.A);
        }

        /// <summary>
        /// 檢查頁面上的註解（Annotation）是否包含行為 (Action)
        /// </summary>
        /// <param name="page">要檢查的 Page 物件</param>
        /// <returns>若任一註解包含 /A 動作則回傳 true，否則回傳 false</returns>
        private static bool CheckPageAnnotations(Page page)
        {
            return page.GetAnnotations().Any(annotation =>
            {
                return annotation.AnnotationDictionary.TryGet(NameToken.A, out var action);
            });
        }
    }
}
