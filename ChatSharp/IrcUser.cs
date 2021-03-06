using System;
using System.Linq;

namespace ChatSharp
{
    public class IrcUser : IEquatable<IrcUser>
    {
        public IrcUser()
        {
        }

        public IrcUser(string host)
        {
            if (!host.Contains("@") && !host.Contains("!"))
                Nick = host;
            else
            {
                string[] mask = host.Split('@', '!');
                Nick = mask[0];
                User = mask[1];
                if (mask.Length <= 2)
                {
                    Hostname = "";
                }
                else
                {
                    Hostname = mask[2];
                }
            }
        }

        public IrcUser(string nick, string user)
        {
            Nick = nick;
            User = user;
            RealName = User;
            Mode = string.Empty;
        }

        public IrcUser(string nick, string user, string password) : this(nick, user)
        {
            Password = password;
        }

        public IrcUser(string nick, string user, string password, string realName) : this(nick, user, password)
        {
            RealName = realName;
        }

        public string Nick { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Mode { get; set; }
        public string RealName { get; set; }
        public string Hostname { get; set; }

        public string Hostmask
        {
            get
            {
                return Nick + "!" + User + "@" + Hostname;
            }
        }

        public bool Match(string mask)
        {
            if (mask.Contains("!") && mask.Contains("@"))
            {
                if (mask.Contains('$'))
                    mask = mask.Remove(mask.IndexOf('$')); // Extra fluff on some networks
                var parts = mask.Split('!', '@');
                if (Match(parts[0], Nick) && Match(parts[1], User) && Match(parts[2], Hostname))
                    return true;
            }
            return false;
        }

        public static bool Match(string mask, string value)
        {
            if (value == null)
                value = string.Empty;
            int i = 0;
            int j = 0;
            for (; j < value.Length && i < mask.Length; j++)
            {
                if (mask[i] == '?')
                    i++;
                else if (mask[i] == '*')
                {
                    i++;
                    if (i >= mask.Length)
                        return true;
                    while (++j < value.Length && value[j] != mask[i]) ;
                    if (j-- == value.Length)
                        return false;
                }
                else
                {
                    if (char.ToUpper(mask[i]) != char.ToUpper(value[j]))
                        return false;
                    i++;
                }
            }
            return i == mask.Length && j == value.Length;
        }

        public bool Equals(IrcUser other)
        {
            return other.Hostmask == Hostmask;
        }

        public override bool Equals(object obj)
        {
            if (obj is IrcUser)
                return Equals((IrcUser)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return Hostmask.GetHashCode();
        }

        public override string ToString()
        {
            return Hostmask;
        }
    }
}
