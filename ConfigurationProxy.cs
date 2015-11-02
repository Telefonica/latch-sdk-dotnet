using System;

namespace LatchSDK
{
    public class ConfigurationProxy
    {
        public int Port { get; private set; }
        public string Domain { get; private set; }
        public string Password { get; private set; }
        public string Host { get; private set; }
        public string User { get; private set; }

        public ConfigurationProxy SetHost(string proxyHost)
        {
            if (String.IsNullOrEmpty(proxyHost))
            {
                throw new ArgumentException("Host value can not be null or empty.");
            }
            this.Host = proxyHost;
            return this;
        }

        public ConfigurationProxy SetPort(int proxyPort)
        {
            if (proxyPort <= 0)
            {
                throw new ArgumentException("Port value can not be less than zero.");
            }
            this.Port = proxyPort;
            return this;
        }

        public ConfigurationProxy SetDomain(string proxyDomain)
        {
            if (!String.IsNullOrEmpty(proxyDomain))
            {
                this.Domain = proxyDomain;
            }
            return this;
        }

        public ConfigurationProxy SetUser(string proxyUser)
        {
            if (!String.IsNullOrEmpty(proxyUser))
            {
                this.User = proxyUser;
            }
            return this;
        }

        public ConfigurationProxy SetPassword(string proxyPassword)
        {
            if (!String.IsNullOrEmpty(proxyPassword))
            {
                this.Password = proxyPassword;
            }
            return this;
        }
    }
}