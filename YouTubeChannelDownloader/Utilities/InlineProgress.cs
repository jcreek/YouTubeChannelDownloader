using System;

namespace YouTubeChannelDownloader.Utilities
{
    internal class InlineProgress : IProgress<double>, IDisposable
    {
        private readonly int _posX;
        private readonly int _posY;
        private string lastProgressPrint = string.Empty;

        public InlineProgress()
        {
            _posX = Console.CursorLeft;
            _posY = Console.CursorTop;
        }

        public void Report(double progress)
        {
            string currentProgress = $"{progress:P1}";
            if (currentProgress != lastProgressPrint)
            {
                Console.SetCursorPosition(_posX, _posY);
                Console.WriteLine(currentProgress);

                lastProgressPrint = currentProgress;
            }
        }

        public void Dispose()
        {
            Console.SetCursorPosition(_posX, _posY);
            Console.WriteLine("Completed ✓");
        }
    }
}
