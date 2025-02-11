using System.Runtime.Serialization;

[DataContract]
public class Machine
{
    [DataMember]
    public string Name;

    public Machine()
    {
        Name = Environment.MachineName.ToLower();
    }
}

[DataContract]
public class AccessKey
{
    [DataMember]
    public string ID;

    [DataMember]
    public string Secret;

    public AccessKey(string id, string secret)
    {
        ID = id;
        Secret = secret;
    }
}

[DataContract]
public class ServiceConfiguration
{
    [DataMember]
    public Machine Machine { get; set; } = new();

    [DataMember]
    public string Domain;

    private string? subDomain;
    [DataMember]
    public string SubDomain
    {
        get => subDomain ?? Machine.Name;
        set => subDomain = string.IsNullOrEmpty(value) ? Machine.Name : value;
    }

    [DataMember]
    public AccessKey AccessKey;

    [DataMember]
    public int IPVersion { get; set; } = 6;

    [DataMember]
    public string Type { get; set; } = "AAAA";
    public string? RecordId { get; internal set; }

    [DataMember]
    public string IpMode { get; set; } = "local";  // 默认使用本地方式获取IP

    public ServiceConfiguration(string domain, AccessKey accessKey, string subDomain, string? recordId = null, string ipMode = "local")
    {
        Domain = domain;
        AccessKey = accessKey;
        SubDomain = subDomain;
        RecordId = recordId;
        IpMode = ipMode;
    }
}