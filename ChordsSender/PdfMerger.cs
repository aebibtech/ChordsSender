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
            var latestChordsPath = new DirectoryInfo(_settings.PDfPath).GetDirectories().OrderByDescending(d => d.LastWriteTime).Select(d => d.Name).First();
            var outputFileName = latestChordsPath + "_chords.pdf";
            var files = Directory.EnumerateFiles(Path.Combine(_settings.PDfPath, latestChordsPath)).ToArray();
            var outputPath = Path.Combine(_settings.PDfPath, outputFileName);

            PdfDocument.MergeFiles(files, outputPath);

            return outputPath;
        }
    }
}
