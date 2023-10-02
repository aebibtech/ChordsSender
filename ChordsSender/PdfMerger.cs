using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordsSender
{
    public class PdfMerger
    {
        private AppSettings _settings;
        public PdfMerger(AppSettings settings)
        {
            _settings = settings;
        }
        public string MergePdf()
        {
            var latestChordsPath = Directory.EnumerateDirectories(_settings.PDfPath).Last();
            var dateFileName = latestChordsPath.Split('\\').Last();
            var outputFileName = dateFileName + "_chords.pdf";
            var files = Directory.EnumerateFiles(Path.Combine(_settings.PDfPath, latestChordsPath));
            var outputPath = Path.Combine(_settings.PDfPath, outputFileName);
            var pdfFiles = new List<PdfDocument>();

            foreach (var file in files)
            {
                pdfFiles.Add(new PdfDocument(file));
            }

            var chordsFile = PdfDocument.Merge(pdfFiles);
            chordsFile.SaveAs(outputPath);

            return outputPath;
        }
    }
}
