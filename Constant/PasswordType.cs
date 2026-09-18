using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekDesk.Constant
{
    public enum PasswordType
    {
        INPUT = 0, //键入密码
        CREATE = 1, //新建密码
        ALTER = 2, //修改密码
        CANCEL = 3, //取消密码
        VERIFY = 4, //验证密码 (进入加密分类/解除加密)
    }
}
