using Newtonsoft.Json;
using UnityEngine;

public class Action
{
    /// <summary>
    /// 要采取的动作类型。
    /// </summary>
    /// <remarks>
    /// 约定的可行值：moveforward, moveback, lookright, lookleft, idle, standup
    /// </remarks>
    [JsonProperty("movement")]
    public string Movement { get; set; }
    [JsonProperty("confidence")]
    public float Confidence { get; set; }
}

public class Cmd
{
    [JsonProperty("timestamp")]
    public long TimestampS { get; set; }
    [JsonProperty("action")]
    public Action Action { get; set; }
}
