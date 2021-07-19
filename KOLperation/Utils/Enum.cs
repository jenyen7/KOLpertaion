using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KOLperation.Utils
{
    public class Enum
    {
        public enum Status
        {
            等公司確認 = 0,
            等KOL確認 = 1,
            雙方確認 = 2,
            公司拒絕 = 3,
            KOL拒絕 = 4,
        }

        public enum Character
        {
            KOL = 0,
            公司 = 1,
        }

        public enum UserType
        {
            不是Google登入用戶 = 0,
            已經註冊過Google的登入用戶 = 1,
            新的Google登入用戶 = 2
        }
    }
}