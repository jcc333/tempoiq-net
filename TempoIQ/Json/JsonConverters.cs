﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using NodaTime.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using TempoIQ.Models;
using TempoIQ.Queries;
using TempoIQ.Results;

namespace TempoIQ.Json
{
    class JsonUtil
    {
        public static string RawJsonField(object key, object value)
        {
            return JsonConvert.SerializeObject(key) + " : " + JsonConvert.SerializeObject(value);
        }

        public static string RawJsonField<T1, T2>(KeyValuePair<T1, T2> pair)
        {
            return JsonUtil.RawJsonField(pair.Key, pair.Value);
        }
    }

    public class DirectionFunctionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(DirectionFunction).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var direction = (DirectionFunction)value;
            string directionString;
            switch (direction)
            {
                case DirectionFunction.Latest:
                    directionString = "latest";
                    break;
                case DirectionFunction.Earliest:
                    directionString = "earliest";
                    break;
                case DirectionFunction.Before:
                    directionString = "before";
                    break;
                case DirectionFunction.After:
                    directionString = "after";
                    break;
                case DirectionFunction.Exact:
                    directionString = "exact";
                    break;
                case DirectionFunction.Nearest:
                    directionString = "nearest";
                    break;
                default:
                    throw new ArgumentException(String.Format("Unrecognized DirectionFunction value: {0}", direction));
                    break;
            }
            writer.WriteValue(directionString);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string valStr = (string)reader.Value;
            switch (valStr)
            {
                case "latest":
                    return DirectionFunction.Latest;
                case "earliest":
                    return DirectionFunction.Earliest;
                case "before":
                    return DirectionFunction.Before;
                case "after":
                    return DirectionFunction.After;
                case "nearest":
                    return DirectionFunction.Nearest;
                case "exact":
                    return DirectionFunction.Exact;
                default:
                    throw new JsonException(String.Format("Unrecognized direction function {0}", valStr));
            }
        }
    }

    public class SelectionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Selection).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var selection = (Selection)value;
            writer.WriteStartObject();
            List<String> Fields = new List<String>();
            foreach (var pair in selection.Selectors) {
                Fields.Add(JsonUtil.RawJsonField(pair));
            }
            writer.WriteRaw(String.Join(",", Fields));
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var selectors = JsonConvert.DeserializeObject<Dictionary<Select.Type, Selector>>((string)reader.Value);
            return new Selection(selectors);
        }
    }

    public class SelectorTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
          return typeof(Select.Type).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Select.Type type = (Select.Type)value;
            if (type == Select.Type.Devices)
                writer.WriteValue("devices");
            else
                writer.WriteValue("sensors");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var stringVal = ((string)reader.Value);
            if (stringVal.Equals("sensors"))
                return Select.Type.Sensors;
            else if (stringVal.Equals("devices"))
                return Select.Type.Devices;
            else
                throw new ArgumentException(String.Format("%s is not a valid selector type", stringVal));
        }
    }

    public class SelectorConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string json = (string)reader.Value;
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject(json) as Dictionary<string, object>;
            dynamic val = null;
            if (dict.TryGetValue("attributes", val))
                return new AttributesSelector(val);
            else if (dict.TryGetValue("key", val))
                return new KeySelector(val);
            else if (dict.TryGetValue("attribute_key", val))
                return new AttributeKeySelector(val);
            else if (dict.TryGetValue("or", val))
                return JsonConvert.DeserializeObject<OrSelector>(json);
            else if (dict.TryGetValue("and", val))
                return JsonConvert.DeserializeObject<AndSelector>(json);
            else
                throw new JsonException("Invalid selector object");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JsonConvert.SerializeObject(value);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsSubclassOf(typeof(Selector));
        }
    }

    public class AllSelectorConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            AllSelector all = (AllSelector)value;
            writer.WriteValue("all");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string[] allKeywords = {"all", "*", "any"};
            if (allKeywords.Any((string s) => s == (string)reader.Value))
                return new AllSelector();
            else 
                throw new JsonException(String.Format("cannot deserialize an AllSelector from %s", (string)reader.Value));
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Selection).IsAssignableFrom(objectType);
        }
    }

    public class ZonedDateTimeConverter : JsonConverter
    {
        private DateTimeZone Zone { get; set; }
        private static LocalDateTimePattern datetimePattern = LocalDateTimePattern.CreateWithInvariantCulture("yyyy-MM-ddTHH:mm:ss.FFF");
        private static OffsetPattern offsetPattern = OffsetPattern.CreateWithInvariantCulture("+HH:mm");

        public ZonedDateTimeConverter()
        {
            this.Zone = DateTimeZone.Utc;
        }

        public ZonedDateTimeConverter(DateTimeZone zone)
        {
            this.Zone = zone;
        }

        public static string ToString(ZonedDateTime datetime)
        {
            var localdatetime = datetime.LocalDateTime;
            var offset = datetime.Offset;
            return String.Format("{0}{1}", datetimePattern.Format(localdatetime), offsetPattern.Format(offset));
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ZonedDateTime).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            else if (!(value is ZonedDateTime))
                throw new ArgumentException(string.Format("Unexpected value when converting. Expected {0}, got {1}.", typeof(ZonedDateTime).FullName, value.GetType().FullName));

            var datetime = (ZonedDateTime)value;
            writer.WriteValue(ZonedDateTimeConverter.ToString(datetime));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                if (objectType != typeof(ZonedDateTime?))
                    throw new Exception(string.Format("Cannot convert null value to {0}.", objectType));
                else
                    return null;

            var offsetDateTimeText = reader.Value.ToString();
            if (string.IsNullOrEmpty(offsetDateTimeText) && objectType == typeof(ZonedDateTime?))
                return null;

            var offsetDateTime = OffsetDateTime.FromDateTimeOffset(DateTimeOffset.Parse(offsetDateTimeText));
            var datetime = new ZonedDateTime(offsetDateTime.ToInstant(), Zone);
            return datetime;
        }
    }

    public class NullableZonedDateTimeConverter : JsonConverter
    {
        private DateTimeZone Zone { get; set; }
        private static LocalDateTimePattern datetimePattern = LocalDateTimePattern.CreateWithInvariantCulture("yyyy-MM-ddTHH:mm:ss.FFF");
        private static OffsetPattern offsetPattern = OffsetPattern.CreateWithInvariantCulture("+HH:mm");

        public NullableZonedDateTimeConverter()
        {
            this.Zone = DateTimeZone.Utc;
        }

        public NullableZonedDateTimeConverter(DateTimeZone zone)
        {
            this.Zone = zone;
        }

        public static string ToString(ZonedDateTime datetime)
        {
            var localdatetime = datetime.LocalDateTime;
            var offset = datetime.Offset;
            return String.Format("{0}{1}", datetimePattern.Format(localdatetime), offsetPattern.Format(offset));
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ZonedDateTime?).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (!(value is ZonedDateTime?))
                throw new ArgumentException(string.Format("Unexpected value when converting. Expected {0}, got {1}.", typeof(ZonedDateTime).FullName, value.GetType().FullName));
            var datetime = (ZonedDateTime?)value;
            if (datetime.HasValue)
                writer.WriteValue(ZonedDateTimeConverter.ToString(datetime.Value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                if (objectType != typeof(ZonedDateTime?))
                    throw new Exception(string.Format("Cannot convert null value to {0}.", objectType));
                else
                    return null;

            var offsetDateTimeText = reader.Value.ToString();
            if (string.IsNullOrEmpty(offsetDateTimeText) && objectType == typeof(ZonedDateTime?))
                return null;

            var offsetDateTime = OffsetDateTime.FromDateTimeOffset(DateTimeOffset.Parse(offsetDateTimeText));
            var datetime = new ZonedDateTime(offsetDateTime.ToInstant(), Zone);
            return datetime;
        }
    }
}
