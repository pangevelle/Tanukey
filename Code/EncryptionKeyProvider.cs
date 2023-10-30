using Microsoft.AspNetCore.Http;
using System;

namespace Tanukey
{
    internal class EncryptionKeyProvider
    {
        private static byte[] staticKey = null;
        static readonly string SESSION_KEY = "TanukeyAesEncryptionKey";
        public static byte[] GetKey(HttpContext httpContext)
        {
            byte[] result = null;
            try
            {
                if (httpContext.Session != null)
                {
                    result = httpContext.Session.Get(SESSION_KEY) as byte[];
                    if (result == null)
                    {
                        result = AesEncryption.GenerateKey();
                        httpContext.Session.Set(SESSION_KEY, result);
                    }
                }
            }
            catch (InvalidOperationException) { }

            if (result == null)
            {
                if (staticKey == null)
                {
                    staticKey = AesEncryption.GenerateKey();
                }
                result = staticKey;
            }

            return result;
        }
    }
}
