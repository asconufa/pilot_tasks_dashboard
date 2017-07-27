using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Ascon.Pilot.Core
{
    public static class AttributeFormatter
    {
        public static Dictionary<string, DValue> Format(MType objectType, Dictionary<string, DValue> values)
        {
            foreach (var value in values)
            {
                var attribute = objectType.Attributes.FirstOrDefault(a => a.Name.Equals(value.Key));
                if (attribute == null)
                    continue;

                var format = GetFormat(attribute.Configuration);
                if (string.IsNullOrEmpty(format))
                    continue;

                try
                {
                    if (value.Value.DoubleValue != null)
                    {
                        value.Value.StrValue = string.Format(format, value.Value.DoubleValue.Value);
                        continue;
                    }

                    if (value.Value.DateValue != null)
                    {
                        value.Value.StrValue = string.Format(format, value.Value.DateValue.Value);
                        continue;
                    }

                    if (value.Value.IntValue != null)
                    {
                        value.Value.StrValue = string.Format(format, value.Value.IntValue.Value);
                        continue;
                    }

                    if (value.Value.StrValue != null)
                    {
                        value.Value.StrValue = string.Format(format, value.Value.StrValue);
                    }

                    if (value.Value.DecimalValue != null)
                    {
                        var cultureName = GetCulture(attribute.Configuration);
                        var cultureInfo = !string.IsNullOrEmpty(cultureName)
                            ? new CultureInfo(cultureName)
                            : CultureInfo.CurrentUICulture;

                        value.Value.StrValue = string.Format(cultureInfo, format, value.Value.DecimalValue);
                    }
                }
                catch (FormatException) { }
            }
            return values;
        }

        public static string GetFormat(string configuration)
        {
            var parsedConfiguration = TryParseXml(configuration);
            var format = parsedConfiguration?.Attributes("Format").FirstOrDefault();
            return format?.Value;
        }

        public static string GetCulture(string configuration)
        {
            var parsedConfiguration = TryParseXml(configuration);
            var format = parsedConfiguration?.Attributes("Culture").FirstOrDefault();
            return format?.Value;
        }

        private static XElement TryParseXml(string xml)
        {
            try
            {
                if (string.IsNullOrEmpty(xml))
                    return null;
                return XElement.Parse(xml);
            }
            catch (System.Xml.XmlException) //<- только ошибки парсинга, все остальное - это проблема!
            {
                return null;
            }
        }
    }
}
