using System;

namespace SQLite_Migrations
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs()
        {
        }

        public ProgressEventArgs(int value, string message)
        {
            Value = value;
            Message = message;
        }

        public int Value { get; set; }
        public string Message { get; set; }
        public object Item { get; set; }
    }
}
