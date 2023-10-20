using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spire.Pdf;

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
            var files = Directory.EnumerateFiles(Path.Combine(_settings.PDfPath, latestChordsPath)).ToArray();
            var outputPath = Path.Combine(_settings.PDfPath, outputFileName);

            PdfDocument.MergeFiles(files, outputPath);

            return outputPath;
        }
    }
}
