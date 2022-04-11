using System;

namespace Client
{

    public enum IMError : byte
    {
        TooUserName = IMClient.IM_TooUsername,
        TooPassword = IMClient.IM_TooPassword,
        Exists = IMClient.IM_Exists,
        NoExists = IMClient.IM_NoExists,
        WrongPassword = IMClient.IM_WrongPass
    }

    public class IMErrorEventArgs : EventArgs
    {
        IMError err;

        public IMErrorEventArgs(IMError error)
        {
            this.err = error;
        }

        public IMError Error
        {
            get { return err; }
        }
    }

    public delegate void IMErrorEventHandler(object sender, IMErrorEventArgs e);
}
