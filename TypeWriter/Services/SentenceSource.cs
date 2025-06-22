using System.IO;

namespace TypeWriter.Services
{
    internal class SentenceSource
    {
        private string[][] _allWords;
        private int _nextMatchCharIndex;
        private int _nextMatchLineIndex;
        private int _nextMatchWordIndex;

        public SentenceSource()
        {
            LoadText(["Hi There! Stand With Ukraine 2024!"]);
        }

        public event Action<(string typedString, string toTypeString)> CharTyped;

        public bool EOF
        {
            get
            {
                return _allWords == null || _allWords.Length == 0 || _nextMatchLineIndex >= _allWords.Length
                    || _nextMatchLineIndex == _allWords.Length - 1 && _nextMatchWordIndex == _allWords[_nextMatchLineIndex].Length - 1 && _nextMatchCharIndex >= _allWords[_nextMatchLineIndex][_nextMatchWordIndex].Length;
            }
        }

        public bool EOL
        {
            get
            {
                return _nextMatchWordIndex >= _allWords[_nextMatchLineIndex].Length;
            }
        }

        public bool EOW
        {
            get
            {
                return _nextMatchCharIndex >= _allWords[_nextMatchLineIndex][_nextMatchWordIndex].Length;
            }
        }

        public string ToTypeString
        {
            get
            {
                return new string(_allWords[_nextMatchLineIndex][_nextMatchWordIndex].Skip(_nextMatchCharIndex).ToArray())
                    + " " + string.Join(" ", _allWords[_nextMatchLineIndex], _nextMatchWordIndex + 1, _allWords[_nextMatchLineIndex].Length - _nextMatchWordIndex - 1);
            }
        }

        public string TypedString
        {
            get
            {
                return (string.Join(' ', _allWords[_nextMatchLineIndex], 0, _nextMatchWordIndex)
                    + " " + new string(_allWords[_nextMatchLineIndex][_nextMatchWordIndex].Take(_nextMatchCharIndex).ToArray())).Trim();
                // 如果是下一个单词的第一个字母，要加空格
            }
        }

        public void LoadText(string path)
        {
            if (Path.GetExtension(path).ToLower() != ".txt")
            {
                throw new ArgumentException("The file extension must be .txt");
            }
            LoadText(File.ReadAllLines(path));
        }

        public void LoadText(IEnumerable<string> lines)
        {
            var lines2 = lines.Where(item => !string.IsNullOrWhiteSpace(item)).Prepend("Hi There! Stand With Ukraine 2022!");
            _allWords = new string[lines2.Count()][];
            int i = 0;
            foreach (var item in lines2)
            {
                _allWords[i] = item.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToArray();
                i++;
            }
            _nextMatchCharIndex = 0;
            _nextMatchWordIndex = 0;
            _nextMatchCharIndex = 0;
            _nextMatchLineIndex = 0;
        }

        public string NextSentence()
        {
            _nextMatchLineIndex++;
            if (_nextMatchLineIndex >= _allWords.Length)
            {
                _nextMatchLineIndex = _allWords.Length;
            }
            _nextMatchWordIndex = 0;
            _nextMatchCharIndex = 0;
            if (_nextMatchLineIndex < _allWords.Length && _allWords.Length > 0)
            {
                return string.Join(' ', _allWords[_nextMatchLineIndex]);
            }
            return null;
        }

        public void OnInputChar(char @char)
        {
            if (EOF)
            {
                //App.Instance.TrayIcon.ShowBalloonTip("", "End Of File", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                Reset();
                return;
            }

            if (char.ToLower(@char) == char.ToLower(_allWords[_nextMatchLineIndex][_nextMatchWordIndex][_nextMatchCharIndex]))
            {
                _nextMatchCharIndex++;
                if (EOW)
                {
                    _nextMatchWordIndex++;
                    _nextMatchCharIndex = 0;
                }
                if (EOL)
                {
                    _nextMatchLineIndex++;
                    _nextMatchWordIndex = 0;
                    _nextMatchCharIndex = 0;
                }
                if (EOF)
                {
                    //App.Instance.TrayIcon.ShowBalloonTip("", "End Of File", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                    Reset();
                }
            }
            else
            {
                //App.Instance.TrayIcon.ShowBalloonTip("", @char.ToString(), Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }

            CharTyped?.Invoke((TypedString, ToTypeString));
        }

        public string PrevSentence()
        {
            _nextMatchLineIndex--;
            if (_nextMatchLineIndex < 0)
            {
                _nextMatchLineIndex = -1;
            }
            _nextMatchWordIndex = 0;
            _nextMatchCharIndex = 0;
            if (_nextMatchLineIndex >= 0 && _allWords.Length > 0)
            {
                return string.Join(' ', _allWords[_nextMatchLineIndex]);
            }
            return null;
        }

        public void Reset()
        {
            _nextMatchCharIndex = 0;
            _nextMatchWordIndex = 0;
            _nextMatchLineIndex = 0;
        }
    }
}
