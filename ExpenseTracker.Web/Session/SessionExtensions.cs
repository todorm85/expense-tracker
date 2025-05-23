﻿using ExpenseTracker.Core.Transactions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExpenseTracker.Web.Session
{
    public static class SessionExtensions
    {
        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = false,
                ReferenceHandler = ReferenceHandler.Preserve,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }
        
        public static void Set<T>(this ISession session, string key, T ts)
        {
            session.Set(key, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(ts, GetJsonOptions())));
        }

        public static T Get<T>(this ISession session, string key) where T : class
        {
            var rawSessionValue = session.Get(key);
            if (rawSessionValue == null) return null;
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(rawSessionValue), GetJsonOptions());
        }
    }
}
