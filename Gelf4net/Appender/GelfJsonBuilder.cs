using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using log4net.Appender;
using log4net.Core;

namespace Esilog.Gelf4net.Appender
{
    class GelfJsonBuilder
    {

        internal string BuildFromLoggingEvent(GelfMessage gelfMessage, Dictionary<string, string> innerAdditionalFields)
        {
            var gelfJsonMessage = JsonConvert.SerializeObject(gelfMessage);
            var jsonObject = JObject.Parse(gelfJsonMessage);
            this.AddAdditionalFields(jsonObject, innerAdditionalFields);
            return jsonObject.ToString();
        }

        private void AddAdditionalFields(JObject jsonObject, Dictionary<string, string> innerAdditionalFields)
        {
            if (innerAdditionalFields == null)
            {
                return;
            }

            foreach (var item in innerAdditionalFields)
            {
                this.AddAdditionalFields(jsonObject, item.Key, item.Value);
            }
        }

        private void AddAdditionalFields(JObject jsonObject, string key, string value)
        {
            if (key == null)
            {
                return;
            }

            if (!key.StartsWith("_"))
            {
                key = String.Format("_{0}", key);
            }

            if (key == "_id")
            {
                return;
            }

            key = Regex.Replace(key, "[\\W]", string.Empty);
            jsonObject.Add(key, value);
        }
    }
}
