using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace Net.LoongTech.OmniCoreX
{

    /// <summary>
    /// PDF 文件处理列
    /// </summary>
    public class PdfHelper
    {
        /// <summary>
        /// 读取PDF文本内容
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string ReadPdf(string fileName)
        {
            string returnValue = string.Empty;

            try
            {
                StringBuilder sbFileContent = new StringBuilder();
                using (PdfReader pdfReader = new PdfReader(fileName))
                using (PdfDocument pdfDoc = new PdfDocument(pdfReader))
                {
                    for (int pageNum = 1; pageNum <= pdfDoc.GetNumberOfPages(); pageNum++)
                    {
                        PdfPage page = pdfDoc.GetPage(pageNum);
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                        string pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                        sbFileContent.AppendLine(pageText);
                    }
                }
                returnValue = sbFileContent.ToString();
            }
            catch
            {
                throw;
            }
            return returnValue;
        }
    }
}
