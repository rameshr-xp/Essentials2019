﻿

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AzureIotHub.Services.Diagnostics
{
    public class Serialization
    {
        // Save memory avoiding serializations that go too deep
        private static readonly JsonSerializerSettings serializationSettings =
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                MaxDepth = 4
            };

        public static string Serialize(object o)
        {
            Dictionary<string, object> logdata = new Dictionary<string, object>();

            // To avoid flooding the logs and logging exceptions, filter
            // exceptions' data and log only what's useful
            foreach (PropertyInfo data in o.GetType().GetRuntimeProperties())
            {
                string name = data.Name;
                object value = data.GetValue(o, index: null);

                if (value is Exception)
                {
                    Exception e = value as Exception;
                    logdata.Add(name, SerializeException(e));
                }
                else
                {
                    logdata.Add(name, value);
                }
            }

            return JsonConvert.SerializeObject(logdata, serializationSettings);
        }

        private static object SerializeException(Exception e, int depth = 3)
        {
            if (e == null)
            {
                return null;
            }

            if (depth == 0)
            {
                return "-max serialization depth reached-";
            }

            AggregateException exception = e as AggregateException;
            if (exception != null)
            {
                List<object> innerExceptions = exception.InnerExceptions
                    .Select(ie => SerializeException(ie, depth - 1)).ToList();

                return new
                {
                    ExceptionFullName = exception.GetType().FullName,
                    ExceptionMessage = exception.Message,
                    exception.StackTrace,
                    exception.Source,
                    exception.Data,
                    InnerExceptions = innerExceptions
                };
            }

            return new
            {
                ExceptionFullName = e.GetType().FullName,
                ExceptionMessage = e.Message,
                e.StackTrace,
                e.Source,
                e.Data,
                InnerException = SerializeException(e.InnerException, depth - 1)
            };
        }
    }
}
