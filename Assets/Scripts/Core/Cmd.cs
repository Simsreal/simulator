using Newtonsoft.Json;
using UnityEngine;

public class Cmd
{
    [JsonProperty("x")]
    public float X { get; set; }
    [JsonProperty("y")]
    public float Y { get; set; }
    /// <summary>
    /// Orientation of the agent
    /// </summary>
    [JsonProperty("orientation")]
    public float Orientation { get; set; }
    /// <summary>
    /// Unix timestamp in millisecond
    /// </summary>
    [JsonProperty("timestamp")]
    public long TimestampMs { get; set; }
    [JsonProperty("get_up")]
    public int GetUp { get; set; }
}
