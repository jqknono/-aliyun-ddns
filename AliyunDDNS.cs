using Aliyun.Acs.Alidns.Model.V20150109;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static Aliyun.Acs.Alidns.Model.V20150109.DescribeDomainRecordsResponse;

public class AliyunDDNSUpdater
{
    private IAcsClient Client { get; set; }
    private readonly ServiceConfiguration config;

    public AliyunDDNSUpdater(ServiceConfiguration sconfig)
    {
        config = sconfig;
        Client = new DefaultAcsClient(DefaultProfile.GetProfile(),
            new Aliyun.Acs.Core.Auth.BasicCredentials(config.AccessKey.ID, config.AccessKey.Secret));
    }

    public bool UpdateDNS()
    {
        int page = 1;
        List<DescribeDomainRecords_Record>? records = GetRecords(page);

        if (records == null)
        {
            return UpdateRecord();
        }

        while (records != null && records.Count != 0)
        {
            foreach (DescribeDomainRecords_Record record in records)
            {
                if (record.DomainName == config.Domain && record.RR == config.SubDomain)
                {
                    // update the record if found
                    return UpdateRecord(record);
                }
            }

            records = GetRecords(++page);
        }

        // add record if not found
        return AddRecord();
    }

    public static string GetIPv6()
    {
        string ipv6 = String.Empty;
        HttpClient client = new(
            new SocketsHttpHandler()
            {
                ConnectCallback = async (context, cancellationToken) =>
                {
                    // Use DNS to look up the IP address(es) of the target host
                    IPHostEntry ipHostEntry = await Dns.GetHostEntryAsync(context.DnsEndPoint.Host);

                    // Filter for IPv6 addresses only
                    Func<IPAddress, bool> predicate = i =>
                                            {
                                                return i.AddressFamily == AddressFamily.InterNetworkV6;
                                            };
                    IPAddress? ipAddress = ipHostEntry?.AddressList.FirstOrDefault(predicate: predicate);

                    // Fail the connection if there aren't any IPV6 addresses
                    if (ipAddress == null)
                    {
                        Task.Delay(10000).Wait();
                        //return Stream.Null;
                        throw new Exception($"No IP4 address for {context.DnsEndPoint.Host}");
                    }

                    // Open the connection to the target host/port
                    TcpClient tcp = new();
                    await tcp.ConnectAsync(ipAddress, context.DnsEndPoint.Port, cancellationToken);

                    // Return the NetworkStream to the caller
                    return tcp.GetStream();
                }
            });
        Task<string>[] tasks =
        {
                client.GetStringAsync("https://6.ipw.cn"),
                client.GetStringAsync("https://api64.ipify.org"),
                client.GetStringAsync("https://ifconfig.co/ip"),
            };
        try
        {
            int index = Task.WaitAny(tasks, TimeSpan.FromSeconds(15));
            if (index == -1)
            {
                return ipv6;
            }

            if (tasks[index].Status == TaskStatus.RanToCompletion)
            {
                ipv6 = tasks[index].Result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return string.Empty;
        }


        return ipv6;
    }

    private List<DescribeDomainRecords_Record>? GetRecords(int page)
    {
        DescribeDomainRecordsRequest request = new()
        {
            DomainName = config.Domain,
#if DEBUG
            PageSize = 10,
#else
            PageSize = 50,
#endif
            PageNumber = page,
            Type = "AAAA",
            Status = "ENABLE"
        };

        try
        {
            DescribeDomainRecordsResponse response = Client.GetAcsResponse(request);
            return response.DomainRecords;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return null;
    }

    private List<DescribeDomainRecords_Record> GetAllRecords()
    {
        List<DescribeDomainRecords_Record> records = new();
        int page = 1;
        List<DescribeDomainRecords_Record>? pageRecords = GetRecords(page);

        while (pageRecords != null && pageRecords.Count != 0)
        {
            records.AddRange(pageRecords);
            pageRecords = GetRecords(++page);
        }

        return records;
    }

    public string ListRecords()
    {
        List<DescribeDomainRecords_Record> records = GetAllRecords();
        if (records.Count == 0)
        {
            return "Failed to get records.";
        }

        StringBuilder sb = new();
        string format = "{0,-20} {1,-20} {2,-6} {3,-20}";
        sb.AppendLine(string.Format(format, "RecordId", "RR", "Type", "Value"));
        foreach (DescribeDomainRecords_Record record in records)
        {
            // add with alignment
            sb.AppendLine(string.Format(format, record.RecordId, record.RR, record.Type, record._Value));
        }

        return sb.ToString();
    }

    private bool UpdateRecord(DescribeDomainRecords_Record? record = null)
    {
        string ip = GetIPv6();
        if (string.IsNullOrEmpty(ip))
        {
            Console.WriteLine("[Error]Can not get ipv6 address, check your network connection.");
            return false;
        }

        if (record != null && record._Value == ip)
        {
            Console.WriteLine("IPv6 address is not changed.");
            return true;
        }

        Console.WriteLine("Update subDomain:" + config.SubDomain + "." + config.Domain + " to=>[" + ip + "]");

        if (string.IsNullOrEmpty(record?.RecordId) && string.IsNullOrEmpty(config.RecordId))
        {
            Console.WriteLine("[Error]Can not get recordId, please check your config.");
            return false;
        }

        UpdateDomainRecordRequest request = new()
        {
            RecordId = config.RecordId ?? record?.RecordId,
            RR = config.SubDomain,
            Type = config.Type,
            _Value = ip,
        };
        try
        {
            UpdateDomainRecordResponse response = Client.GetAcsResponse(request);
            return response.HttpResponse.isSuccess();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return false;
    }

    private bool AddRecord()
    {
        string ip = GetIPv6();
        if (string.IsNullOrEmpty(ip))
        {
            Console.WriteLine("Can not get ipv6 address.");
            return false;
        }

        Console.WriteLine("Add subDomain:" + config.SubDomain + "." + config.Domain + " to=>[" + ip + "]");

        AddDomainRecordRequest request = new()
        {
            DomainName = config.Domain,
            RR = config.SubDomain,
            Type = config.Type,
            _Value = ip,
        };

        try
        {
            AddDomainRecordResponse response = Client.GetAcsResponse(request);
            return response.HttpResponse.isSuccess();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return false;
    }
}