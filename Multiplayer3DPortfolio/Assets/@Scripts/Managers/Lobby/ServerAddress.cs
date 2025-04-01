using System;

/// <summary>
///  for display anonymous Relay IP
/// </summary>
public class ServerAddress : IEquatable<ServerAddress>
{
    string _ip;
    string _port;

    public string IP => _ip;
    public string Port => _port;

    public ServerAddress(string ip, string port)
    {
        _ip = ip;
        _port = port;
    }

    public bool Equals(ServerAddress other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _ip == other._ip && _port == other._port;
    }

#pragma warning disable CS0659  // 경고 비활성화
    public override bool Equals(object obj)
#pragma warning restore CS0659  // 경고 활성화
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ServerAddress)obj);
    }

}
