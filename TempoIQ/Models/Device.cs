﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TempoIQ.Json;
using TempoIQ.Results;
using TempoIQ.Utilities;

namespace TempoIQ.Models
{
    /// <summary>
    /// The TempoIQ notion of a device.
    /// Devices have sensors.
    /// </summary>
    [JsonObject]
    public class Device : IModel
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("attributes")]
        public IDictionary<string, string> Attributes { get; set; }

        [JsonProperty("sensors")]
        public IList<Sensor> Sensors { get; set; }

        [JsonIgnore]
        public static string CursoredMediaTypeVersion
        {
            get { return "application/prs.tempoiq.datapoint-collection.v2"; }
        }

        [JsonConstructor]
        public Device(string key, string name, IDictionary<string, string> attributes, IList<Sensor> sensors)
        {
            this.Key = key;
            this.Name = name;
            this.Attributes = attributes;
            this.Sensors = sensors;
        }

        public Device(string key)
        {
            this.Key = key;
            this.Name = "";
            this.Attributes = new Dictionary<string, string>();
            this.Sensors = new List<Sensor>();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if (obj == this)
                return true;
            else if (obj is Device)
                return this.Equals((Device)obj);
            else
                return false;
        }

        public bool Equals(Device that)
        {
            return this.Key.Equals(that.Key)
                && this.Attributes.SequenceEqual(that.Attributes)
                && this.Name.Equals(that.Name)
                && this.Sensors.SequenceEqual(that.Sensors);
        }

        public override int GetHashCode()
        {
            int hash = HashCodeHelper.Initialize();
            hash = HashCodeHelper.Hash(hash, Attributes);
            hash = HashCodeHelper.Hash(hash, Key);
            hash = HashCodeHelper.Hash(hash, Name);
            hash = HashCodeHelper.Hash(hash, Sensors);
            return hash;
        }
    }
}