using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freshstore.bll
{
    public static class Messages
    {
        public const string NOT_FOUND_MESSAGE = "Record could not be found.";
        public const string SELECT_ERROR = "Record could not be selected, please contact your system administrator.";
        public const string INSERT_ERROR = "Record could not be inserted, please contact your system administrator.";
        public const string UPDATE_ERROR = "Record could not be updated, please contact your system administrator.";
        public const string DELETE_ERROR = "Record could not be deleted, please contact your system administrator.";

        public const string LOG_HIT = "HIT";
        public const string LOG_MISS = "MISS";
    }
}
