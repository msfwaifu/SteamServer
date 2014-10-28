/*
	This project is licensed under the GPL 2.0 license. Please respect that.

	Initial author: (https://github.com/)Convery
	Started: 2014-10-28
	Notes:
		This logging class is based on NoFates logging class from a few years ago.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace SteamServer
{
    // Severity.
    public enum LogLevel
    {
        None = 0,
        Debug = 1,
        Info = 2,
        Warning = 4,
        Error = 8,
        Data = 16,
        All = 31
    }

    class Logging
    {
    }
}
