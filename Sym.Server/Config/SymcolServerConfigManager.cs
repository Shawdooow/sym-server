using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace Sym.Server.Config
{
    public class SymcolServerConfigManager : IniConfigManager<SymcolServerSetting>
    {
        protected override string Filename => @"SymcolServer.ini";

        public SymcolServerConfigManager(Storage storage) : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            Set(SymcolServerSetting.HostIP, "Host's IP Address");
            Set(SymcolServerSetting.LocalIP, "Local IP Address");

            Set(SymcolServerSetting.HostPort, 25570);
            Set(SymcolServerSetting.LocalPort, 25570);
        }
    }

    public enum SymcolServerSetting
    {
        HostIP,
        LocalIP,

        HostPort,
        LocalPort
    } 
}