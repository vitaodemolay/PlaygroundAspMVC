using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PubliAX.Web.Configs.Toolkit
{
    public static class Resource
    {
        public static string GetScriptToGetUserByCPF(string cpfOnlyNumber)
        {
            return $"SELECT * FROM [dbo].[VW$User001] WHERE [dbo].ONLYNUMBER(cnpfj) = '{cpfOnlyNumber}'";
        }
    }
}