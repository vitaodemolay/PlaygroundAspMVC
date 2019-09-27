using System;

namespace PlaygroundAspMVC.MvcAuthTeste.Config.Parameters
{
    public static class SecurityParameters
    {
        public static void Init()
        {
            SystemPasswordForRequestTokenApiAuthorization = Guid.NewGuid();
        }


        public static Guid SystemPasswordForRequestTokenApiAuthorization { get; private set; }
    }
}