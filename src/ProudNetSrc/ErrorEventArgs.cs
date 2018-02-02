using System;

namespace ProudNetSrc
{
    public class ErrorEventArgs : EventArgs
    {
        public ErrorEventArgs(Exception exception)
            : this(null, exception)
        {
        }

        public ErrorEventArgs(ProudSession session, Exception exception)
        {
            Session = session;
            Exception = exception;
        }

        public ProudSession Session { get; }
        public Exception Exception { get; }
    }
}
