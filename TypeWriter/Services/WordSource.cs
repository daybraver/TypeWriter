using System.IO;

namespace TypeWriter.Services
{
    internal class WordSource
    {
        private int _index;
        private List<string> _words;

        public WordSource()
        {
            _words = new List<string>();
        }

        public string Word
        {
            get
            {
                if (_words == null || _words.Count == 0 || _index < 0 || _index >= _words.Count)
                {
                    return string.Empty;
                }
                return _words[_index];
            }
        }

        public void LoadWords(IEnumerable<string> words)
        {
            _words.Clear();
            foreach (string word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    continue;
                }
                _words.Add(word.Trim());
            }
            _index = -1;
        }

        public void LoadWords(string path)
        {
            LoadWords(File.ReadAllLines(path));
        }

        public void Next()
        {
            if (_words == null || _words.Count == 0)
            {
                _index = -1;
            }
            else if (_index >= _words.Count)
            {
                _index = _words.Count;
            }
            else
            {
                _index++;
            }
        }

        public void Previous()
        {
            if (_words == null || _words.Count == 0)
            {
                _index = -1;
            }
            else if (_index < 0)
            {
                _index = -1;
            }
            else
            {
                _index--;
            }
        }
    }
}
