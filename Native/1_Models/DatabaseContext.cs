using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Reflection;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace JDroidXDesk.Native
{
    public class DatabaseContext
    {
        public string DbPath { get; private set; }
        public AppSettings Settings { get; set; }
        public ServerConfigModel ServerConfig { get; set; }
        public List<DeviceItem> Devices { get; set; }
        public List<GroupItem> Groups { get; set; }
        public List<SessionItem> Sessions { get; set; }
        public List<GuardianItem> Guardians { get; set; }
        public List<SafeZoneItem> SafeZones { get; set; }
        public List<ContactItem> Contacts { get; set; }
        public List<NotificationItem> Notifications { get; set; }
        public List<LogItem> Logs { get; set; }

        private JavaScriptSerializer serializer;

        public DatabaseContext()
        {
            serializer = new JavaScriptSerializer();
            DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.json");
            Load();
        }

        public void Load()
        {
            if (!File.Exists(DbPath))
            {
                InstallationManager.ExtractEmbeddedResource("database.json", DbPath, false);
            }

            if (File.Exists(DbPath))
            {
                try
                {
                    string json = File.ReadAllText(DbPath);
                    Dictionary<string, object> dict = serializer.Deserialize<Dictionary<string, object>>(json);

                    if (dict.ContainsKey("settings"))
                    {
                        string sJson = serializer.Serialize(dict["settings"]);
                        Settings = serializer.Deserialize<AppSettings>(sJson);
                    }
                    else Settings = new AppSettings();

                    if (Settings == null) Settings = new AppSettings();
                    if (Settings.connection == null) Settings.connection = new ConnectionSettings();
                    string curBase = AppDomain.CurrentDomain.BaseDirectory;
                    if (Settings.connection.localInstalledFolder != curBase)
                    {
                        Settings.connection.localInstalledFolder = curBase;
                        Settings.connection.localRelayScriptPath = Path.Combine(curBase, "colab_relay_server.py");
                    }

                    if (dict.ContainsKey("serverConfig") && dict["serverConfig"] != null)
                    {
                        try
                        {
                            string scJson = serializer.Serialize(dict["serverConfig"]);
                            ServerConfig = serializer.Deserialize<ServerConfigModel>(scJson);
                        }
                        catch { ServerConfig = null; }
                    }
                    if (ServerConfig == null) ServerConfig = ServerConfigModel.CreateDefault();
                    if (ServerConfig.processingLimits == null) ServerConfig.processingLimits = new ProcessingLimitsModel();
                    if (ServerConfig.engines == null || ServerConfig.engines.Count == 0) ServerConfig.engines = ServerConfigModel.CreateDefault().engines;

                    Devices = DeserializeList<DeviceItem>(dict, "devices");
                    Groups = DeserializeList<GroupItem>(dict, "groups");
                    Sessions = DeserializeList<SessionItem>(dict, "sessions");
                    Guardians = DeserializeList<GuardianItem>(dict, "guardians");
                    SafeZones = DeserializeList<SafeZoneItem>(dict, "safeZones");
                    Contacts = DeserializeList<ContactItem>(dict, "contacts");
                    Notifications = DeserializeList<NotificationItem>(dict, "notifications");
                    Logs = DeserializeList<LogItem>(dict, "logs");

                    // Relational OOPS MVC Resolution: populate device group name dynamically from groups table
                    if (Devices != null && Groups != null)
                    {
                        foreach (DeviceItem d in Devices)
                        {
                            if (string.IsNullOrEmpty(d.group) && !string.IsNullOrEmpty(d.groupId))
                            {
                                GroupItem g = Groups.Find(x => x.id == d.groupId);
                                if (g != null) d.group = g.name;
                            }
                            else if (string.IsNullOrEmpty(d.groupId) && !string.IsNullOrEmpty(d.group))
                            {
                                GroupItem g = Groups.Find(x => x.name == d.group);
                                if (g != null) d.groupId = g.id;
                            }
                        }
                    }

                    // Automatic Log Retention Policy Enforcement
                    if (Logs != null && Logs.Count > 0 && ServerConfig != null && ServerConfig.processingLimits != null)
                    {
                        int retentionDays = ServerConfig.processingLimits.logRetentionDays > 0 ? ServerConfig.processingLimits.logRetentionDays : 7;
                        DateTime cutoff = DateTime.Now.AddDays(-retentionDays);
                        List<LogItem> validLogs = new List<LogItem>();
                        foreach (LogItem item in Logs)
                        {
                            DateTime dt;
                            if (DateTime.TryParse(item.timestamp, out dt))
                            {
                                if (dt >= cutoff) validLogs.Add(item);
                            }
                            else validLogs.Add(item);
                        }
                        if (validLogs.Count < Logs.Count)
                        {
                            Logs = validLogs;
                        }
                    }

                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Database Load Exception: " + ex.Message);
                }
            }

            InitDefaults();
        }

        private List<T> DeserializeList<T>(Dictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                string s = serializer.Serialize(dict[key]);
                return serializer.Deserialize<List<T>>(s) ?? new List<T>();
            }
            return new List<T>();
        }

        private void InitDefaults()
        {
            Settings = new AppSettings();
            ServerConfig = ServerConfigModel.CreateDefault();
            Devices = new List<DeviceItem>();
            Groups = new List<GroupItem> {
                new GroupItem { id = "grp-1", name = "Workstations", desc = "Saved remote workstations" }
            };
            Sessions = new List<SessionItem>();
            Guardians = new List<GuardianItem>();
            SafeZones = new List<SafeZoneItem>();
            Contacts = new List<ContactItem>();
            Notifications = new List<NotificationItem>();
            Logs = new List<LogItem>();
            Save();
        }

        public void Save()
        {
            try
            {
                Dictionary<string, object> root = new Dictionary<string, object>();
                root["settings"] = Settings;
                root["serverConfig"] = ServerConfig;
                root["devices"] = Devices;
                root["groups"] = Groups;
                root["sessions"] = Sessions;
                root["guardians"] = Guardians;
                root["safeZones"] = SafeZones;
                root["contacts"] = Contacts;
                root["notifications"] = Notifications ?? new List<NotificationItem>();
                root["logs"] = Logs;

                string json = serializer.Serialize(root);
                File.WriteAllText(DbPath, json);
                SyncYaml();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Database Save Exception: " + ex.Message);
            }
        }

        public void SyncYaml()
        {
            try
            {
                string yamlPath = "windows_rdp.yaml";
                if (!File.Exists(yamlPath))
                {
                    string alt = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "windows_rdp.yaml");
                    if (File.Exists(alt)) yamlPath = alt;
                }
                if (File.Exists(yamlPath) && Settings != null && Settings.connection != null)
                {
                    string content = File.ReadAllText(yamlPath);
                    if (Settings.connection.relayPort > 0)
                    {
                        content = Regex.Replace(content, @"relay_port:\s*\d+", "relay_port: " + Settings.connection.relayPort);
                    }
                    if (!string.IsNullOrEmpty(Settings.connection.customDomain))
                    {
                        content = Regex.Replace(content, @"custom_domain:\s*[""'].*?[""']", "custom_domain: \"" + Settings.connection.customDomain + "\"");
                    }
                    if (Settings.connection.tunnelToken != null)
                    {
                        if (content.Contains("tunnel_token:"))
                        {
                            content = Regex.Replace(content, @"tunnel_token:\s*[""'].*?[""']", "tunnel_token: \"" + Settings.connection.tunnelToken + "\"");
                        }
                    }
                    File.WriteAllText(yamlPath, content);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("YAML Sync Exception: " + ex.Message);
            }
        }

        public void AddLog(string type, string action, string details)
        {
            Logs.Insert(0, new LogItem {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                type = type,
                action = action,
                details = details
            });
            if (Logs.Count > 200) Logs.RemoveAt(Logs.Count - 1);
            Save();
        }
    }

    // ==========================================================================
    // CLOUDFLARE TUNNEL CONTROLLER ENGINE (WINDOWS INGRESS & REVERSE PROXY)
    // ==========================================================================
}
