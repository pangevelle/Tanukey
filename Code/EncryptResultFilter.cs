using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Reflection;

namespace Tanukey
{
    internal class EncryptResultFilter : IResultFilter
    {
        byte[] encryptionKey; 
        public void OnResultExecuting(ResultExecutingContext context)
        {
            encryptionKey = EncryptionKeyProvider.GetKey(context.HttpContext);
            EncryptAnnotatedProperties(context.Result);
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            // Vous pouvez ajouter du code ici pour être exécuté après le résultat
        }

        private void EncryptAnnotatedProperties(IActionResult result)
        {
            if (result is ViewResult viewResult)
            {
                EncryptObjectProperties(viewResult.Model);
            }
        }

        private void EncryptObjectProperties(object obj)
        {
            if (obj != null)
            {
                Type objectType = obj.GetType();
                PropertyInfo[] properties = objectType.GetProperties();
                foreach (var property in properties)
                {
                    var encryptAttribute = property.GetCustomAttribute<ObfuscatedAttribute>();
                    if ((encryptAttribute != null) && (property.GetValue(obj) != null))
                    {
                        var encryptedValue = new AesEncryption(encryptionKey).Encrypt(property.GetValue(obj).ToString());
                        if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(obj, encryptedValue);
                        }
                        else if (property.PropertyType.IsClass)
                        {
                            Type type = property.PropertyType;
                            var value = Activator.CreateInstance(type, new object[] { encryptedValue });
                            property.SetValue(obj, value);
                        }
                        else
                            throw new Exception();
                    }
                    // Si la propriété est un sous-objet, récursion
                    if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        EncryptObjectProperties(property.GetValue(obj));
                    }
                }
            }
        }
    }

}