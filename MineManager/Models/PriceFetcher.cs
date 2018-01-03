using System;

namespace MineManager.Models
{
    public class PriceFetcher
    {
        private readonly Func<string, string> _fetcher;

        public PriceFetcher(Func<string, string> function)
        {
            _fetcher = function;
        }

        public string ConvertText(string inputText)
        {
            return _fetcher(inputText);
        }
    }
}
