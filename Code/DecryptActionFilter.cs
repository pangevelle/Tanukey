using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Tanukey
{
    internal class DecryptActionFilter : IActionFilter
    {
        private byte[] encryptionKey;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            encryptionKey = EncryptionKeyProvider.GetKey(context.HttpContext);
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                DecryptParameters(context.ActionArguments, controllerActionDescriptor);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Vous pouvez ajouter du code ici pour être exécuté après l'action
        }

        private void DecryptParameters(IDictionary<string, object> parameters, ControllerActionDescriptor controllerActionDescriptor)
        {
            var decryptedParameters = new Dictionary<string, object>();

            foreach (var param in parameters)
            {
                var parameter = controllerActionDescriptor.Parameters.FirstOrDefault(p => p.Name == param.Key);

                if (parameter != null && IsPrimitiveType(param.Value.GetType()) && HasEncryptAttribute(controllerActionDescriptor, parameter))
                {
                    var decryptedValue = new AesEncryption(encryptionKey).Decrypt(param.Value.ToString());
                    decryptedParameters.Add(param.Key, decryptedValue);
                }
                else if (parameter != null)
                {
                    DecryptObject(param.Value);
                    decryptedParameters.Add(param.Key, param.Value);
                }
                else
                {
                    decryptedParameters.Add(param.Key, param.Value);
                }
            }

            parameters.Clear();
            foreach (var entry in decryptedParameters)
            {
                parameters.Add(entry.Key, entry.Value);
            }
        }


        private void DecryptObject(object obj)
        {
            if (obj != null)
            {
                Type objectType = obj.GetType();
                PropertyInfo[] properties = objectType.GetProperties();
                foreach (var property in properties)
                {
                    var rowValue = property.GetValue(obj);
                    if (HasEncryptAttribute(property) && (rowValue != null) && (property.PropertyType == typeof(string)))
                    {
                        property.SetValue(obj, new AesEncryption(encryptionKey).Decrypt(rowValue.ToString()));
                    }
                    else if ((HasEncryptAttribute(property) && (rowValue != null)))
                    {
                        Type type = property.PropertyType;
                        var value = Activator.CreateInstance(type, new object[] { new AesEncryption(encryptionKey).Decrypt(rowValue.ToString()) });
                        property.SetValue(obj, value);
                    }
                    else if (!IsPrimitiveType(property.PropertyType))
                    {
                        DecryptObject(rowValue);
                    }
                }
            }
        }

        private bool HasEncryptAttribute(ControllerActionDescriptor controllerActionDescriptor, ParameterDescriptor parameter)
        {
            var methodInfo = controllerActionDescriptor.MethodInfo;
            var parameterInfo = methodInfo.GetParameters().FirstOrDefault(p => p.Name == parameter.Name);

            if (parameterInfo != null)
            {
                return parameterInfo.GetCustomAttributes(typeof(ObfuscatedAttribute), false).Any();
            }

            return false;
        }

        private bool HasEncryptAttribute(PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(ObfuscatedAttribute), false).Any();
        }

        private bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive || type.IsValueType || type == typeof(string);
        }
    }
}