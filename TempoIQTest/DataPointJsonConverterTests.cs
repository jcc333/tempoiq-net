﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Newtonsoft.Json;
using TempoIQ.Models;
using TempoIQ.Json;

namespace TempoIQTest.Json
{
    [TestClass]
    public class SerializeDatapointJsonConverterTests
    {
        [TestMethod]
        public void UtcTest()
        {
            var zone = DateTimeZone.Utc;
            var converter = new ZonedDateTimeConverter();
            var dataPoint = JsonConvert.DeserializeObject<DataPoint>("{\"timestamp\":\"2012-01-01T00:00:01.000+00:00\",\"value\":12.34}", converter);
            var time = new LocalDateTime(2012, 1, 1, 0, 0, 1);
            var expected = new DataPoint(zone.AtStrictly(time), 12.34);
            Assert.AreEqual(expected, dataPoint);
        }

        [TestMethod]
        public void TimeZoneTest()
        {
            var zone = DateTimeZoneProviders.Tzdb["America/Chicago"];
            var converter = new ZonedDateTimeConverter(zone);
            var dataPoint = JsonConvert.DeserializeObject<DataPoint>("{\"timestamp\":\"2012-01-01T00:00:01.000-06:00\",\"value\":12.34}", converter);
            var time = new LocalDateTime(2012, 1, 1, 0, 0, 1);
            var expected = new DataPoint(zone.AtStrictly(time), 12.34);
            Assert.AreEqual(expected, dataPoint);
        }
    }

    [TestClass]
    public class DeserializeDatapointJsonConverterTests
    {
        [TestMethod]
        public void UtcSerializeTest()
        {
            var zone = DateTimeZone.Utc;
            var converter = new ZonedDateTimeConverter();
            string inbound = "{\"timestamp\":\"2012-01-01T00:00:01.000-06:00\",\"value\":12.34}";
            var dataPointIn = JsonConvert.DeserializeObject<DataPoint>(inbound, converter);
            string outbound = JsonConvert.SerializeObject(dataPointIn, converter);
            var dataPointOut = JsonConvert.DeserializeObject<DataPoint>(outbound, converter);
            Assert.AreEqual(dataPointIn, dataPointOut);
        }

        [TestMethod]
        public void ZonedSerializeTest()
        {
            var zone = DateTimeZoneProviders.Tzdb["America/Chicago"];
            var converter = new ZonedDateTimeConverter(zone);
            string inbound = "{\"timestamp\":\"2012-01-01T00:00:01.000-06:00\",\"value\":12.34}";
            var dataPointIn = JsonConvert.DeserializeObject<DataPoint>(inbound, converter);
            string outbound = JsonConvert.SerializeObject(dataPointIn, converter);
            var dataPointOut = JsonConvert.DeserializeObject<DataPoint>(outbound, converter);
            Assert.AreEqual(dataPointIn, dataPointOut);
        }
    }
}