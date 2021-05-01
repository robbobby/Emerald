using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using Server.Library.Utils;
using Server.MirDatabase;
using Server.MirNetwork;
using Server.MirObjects;
using S = ServerPackets;

namespace Server.MirEnvir {
    public class MobThread {
        public int Id = 0;
        public long LastRunTime = 0;
        public long StartTime = 0;
        public long EndTime = 0;
        public LinkedList<MapObject> ObjectsList = new LinkedList<MapObject>();
        public LinkedListNode<MapObject> _current = null;
        public bool Stop = false;
    }

    public class RandomProvider {
        private static int seed = Environment.TickCount;
        private static readonly ThreadLocal<Random> RandomWrapper = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static Random GetThreadRandom() =>
            RandomWrapper.Value;

        public int Next() =>
            RandomWrapper.Value.Next();
        public int Next(int maxValue) =>
            RandomWrapper.Value.Next(maxValue);
        public int Next(int minValue, int maxValue) =>
            RandomWrapper.Value.Next(minValue, maxValue);
    }

    public class Envir {
        public static Envir Main { get; } = new Envir();

        public static Envir Edit { get; } = new Envir();

        protected static MessageQueue MessageQueue =>
            MessageQueue.Instance;

        public static object AccountLock = new object();
        public static object LoadLock = new object();

        public const int Version = 82;
        public const int CustomVersion = 0;
        public static readonly string DatabasePath = Path.Combine(".", "Server.MirDB");
        public static readonly string AccountPath = Path.Combine(".", "Server.MirADB");
        public static readonly string BackUpPath = Path.Combine(".", "Back Up");
        public bool ResetGS = false;

        private static readonly Regex AccountIDReg, PasswordReg, EMailReg, CharacterReg;

        public static int LoadVersion;
        public static int LoadCustomVersion;

        private readonly DateTime _startTime = DateTime.Now;
        public readonly Stopwatch Stopwatch = Stopwatch.StartNew();

        public long Time { get; private set; }
        public RespawnTimer RespawnTick = new RespawnTimer();
        private static List<string> DisabledCharNames = new List<string>();

        public DateTime Now =>
            _startTime.AddMilliseconds(Time);

        public bool Running { get; private set; }


        private static uint _objectID;
        public uint ObjectID => ++_objectID;

        public static int _playerCount;
        public int PlayerCount => Players.Count;

        public RandomProvider Random = new RandomProvider();


        private Thread _thread;
        private TcpListener _listener;
        private bool StatusPortEnabled = true;
        public List<MirStatusConnection> StatusConnections = new List<MirStatusConnection>();
        private TcpListener _StatusPort;
        private int _sessionID;
        public List<MirConnection> Connections = new List<MirConnection>();


        //Server DB
        public int MapIndex, ItemIndex, MonsterIndex, NPCIndex, QuestIndex, GameshopIndex, ConquestIndex, RespawnIndex;
        public List<MapInfo> MapInfoList = new List<MapInfo>();
        public List<ItemInfo> ItemInfoList = new List<ItemInfo>();
        public List<MonsterInfo> MonsterInfoList = new List<MonsterInfo>();
        public List<NPCInfo> NPCInfoList = new List<NPCInfo>();
        public DragonInfo DragonInfo = new DragonInfo();
        public Magic MagicEnvir { get; } = new Magic();
        public List<QuestInfo> QuestInfoList = new List<QuestInfo>();
        public List<GameShopItem> GameShopList = new List<GameShopItem>();
        public List<RecipeInfo> RecipeInfoList = new List<RecipeInfo>();
        public Dictionary<int, int> GameshopLog = new Dictionary<int, int>();

        //User DB
        public int NextAccountID, NextCharacterID;
        public ulong NextUserItemID, NextAuctionID, NextMailID;
        public List<AccountInfo> AccountList = new List<AccountInfo>();
        public List<CharacterInfo> CharacterList = new List<CharacterInfo>();
        public LinkedList<AuctionInfo> Auctions = new LinkedList<AuctionInfo>();
        public int GuildCount, NextGuildID;
        public List<GuildObject> GuildList = new List<GuildObject>();


        //Live Info
        public List<Map> MapList = new List<Map>();
        public List<SafeZoneInfo> StartPoints = new List<SafeZoneInfo>();
        public List<ItemInfo> StartItems = new List<ItemInfo>();
        public List<MailInfo> Mail = new List<MailInfo>();
        public List<PlayerObject> Players = new List<PlayerObject>();
        public bool Saving = false;
        public LightSetting Lights;
        public LinkedList<MapObject> Objects = new LinkedList<MapObject>();

        public List<ConquestInfo> ConquestInfos = new List<ConquestInfo>();
        public List<ConquestObject> Conquests = new List<ConquestObject>();



        //multithread vars
        readonly object _locker = new object();
        public MobThread[] MobThreads = new MobThread[Settings.ThreadLimit];
        private Thread[] MobThreading = new Thread[Settings.ThreadLimit];
        public int spawnmultiplyer = 1; //set this to 2 if you want double spawns (warning this can easely lag your server far beyond what you imagine)

        public List<string> CustomCommands = new List<string>();
        public Dragon DragonSystem;
        public NPCObject DefaultNPC;
        public NPCObject MonsterNPC;
        public NPCObject RobotNPC;

        private readonly List<DropInfo> _fishingDrops = new List<DropInfo>();
        private readonly List<DropInfo> _awakeningDrops = new List<DropInfo>();

        private readonly List<DropInfo> _strongboxDrops = new List<DropInfo>();
        private readonly List<DropInfo> _blackstoneDrops = new List<DropInfo>();

        public readonly List<GuildAtWar> GuildsAtWar = new List<GuildAtWar>();
        public readonly List<MapRespawn> SavedSpawns = new List<MapRespawn>();

        private readonly List<Rank_Character_Info> RankTop = new List<Rank_Character_Info>();
        private readonly List<Rank_Character_Info>[] _rankClass = new List<Rank_Character_Info>[5];
        public readonly int[] RankBottomLevel = new int[6];
        private static HttpServer _http;
        static Envir() {
            AccountIDReg =
                new Regex(@"^[A-Za-z0-9]{" + Globals.MinAccountIDLength + "," + Globals.MaxAccountIDLength + "}$");
            PasswordReg =
                new Regex(@"^[A-Za-z0-9]{" + Globals.MinPasswordLength + "," + Globals.MaxPasswordLength + "}$");
            EMailReg = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            CharacterReg =
                new Regex(@"^[\u4e00-\u9fa5_A-Za-z0-9]{" + Globals.MinCharacterNameLength + "," + Globals.MaxCharacterNameLength +
                    "}$");

            var path = Path.Combine(Settings.EnvirPath, "DisabledChars.txt");
            DisabledCharNames.Clear();
            if (!File.Exists(path)) {
                File.WriteAllText(path, "");
            } else {
                var lines = File.ReadAllLines(path);

                for (int i = 0; i < lines.Length; i++) {
                    if (lines[i].StartsWith(";") || string.IsNullOrWhiteSpace(lines[i])) continue;
                    DisabledCharNames.Add(lines[i].ToUpper());
                }
            }
        }

        public static int LastCount = 0, LastRealCount = 0;
        public static long LastRunTime = 0;
        public int MonsterCount;

        private long warTime, mailTime, guildTime, conquestTime, rentalItemsTime;
        private int DailyTime = DateTime.Now.Day;

        
        
        private string CanStartEnvir() {
            if (Settings.EnforceDBChecks) {
                if (StartPoints.Count == 0) return "Cannot start server without start points";

                if (GetMonsterInfo(Settings.SkeletonName, true) == null) return "Cannot start server without mob: " + Settings.SkeletonName;
                if (GetMonsterInfo(Settings.ShinsuName, true) == null) return "Cannot start server without mob: " + Settings.ShinsuName;
                if (GetMonsterInfo(Settings.BugBatName, true) == null) return "Cannot start server without mob: " + Settings.BugBatName;
                if (GetMonsterInfo(Settings.Zuma1, true) == null) return "Cannot start server without mob: " + Settings.Zuma1;
                if (GetMonsterInfo(Settings.Zuma2, true) == null) return "Cannot start server without mob: " + Settings.Zuma2;
                if (GetMonsterInfo(Settings.Zuma3, true) == null) return "Cannot start server without mob: " + Settings.Zuma3;
                if (GetMonsterInfo(Settings.Zuma4, true) == null) return "Cannot start server without mob: " + Settings.Zuma4;
                if (GetMonsterInfo(Settings.Zuma5, true) == null) return "Cannot start server without mob: " + Settings.Zuma5;
                if (GetMonsterInfo(Settings.Zuma6, true) == null) return "Cannot start server without mob: " + Settings.Zuma6;
                if (GetMonsterInfo(Settings.Zuma7, true) == null) return "Cannot start server without mob: " + Settings.Zuma7;
                if (GetMonsterInfo(Settings.Turtle1, true) == null) return "Cannot start server without mob: " + Settings.Turtle1;
                if (GetMonsterInfo(Settings.Turtle2, true) == null) return "Cannot start server without mob: " + Settings.Turtle2;
                if (GetMonsterInfo(Settings.Turtle3, true) == null) return "Cannot start server without mob: " + Settings.Turtle3;
                if (GetMonsterInfo(Settings.Turtle4, true) == null) return "Cannot start server without mob: " + Settings.Turtle4;
                if (GetMonsterInfo(Settings.Turtle5, true) == null) return "Cannot start server without mob: " + Settings.Turtle5;
                if (GetMonsterInfo(Settings.BoneMonster1, true) == null) return "Cannot start server without mob: " + Settings.BoneMonster1;
                if (GetMonsterInfo(Settings.BoneMonster2, true) == null) return "Cannot start server without mob: " + Settings.BoneMonster2;
                if (GetMonsterInfo(Settings.BoneMonster3, true) == null) return "Cannot start server without mob: " + Settings.BoneMonster3;
                if (GetMonsterInfo(Settings.BoneMonster4, true) == null) return "Cannot start server without mob: " + Settings.BoneMonster4;
                if (GetMonsterInfo(Settings.BehemothMonster1, true) == null) return "Cannot start server without mob: " + Settings.BehemothMonster1;
                if (GetMonsterInfo(Settings.BehemothMonster2, true) == null) return "Cannot start server without mob: " + Settings.BehemothMonster2;
                if (GetMonsterInfo(Settings.BehemothMonster3, true) == null) return "Cannot start server without mob: " + Settings.BehemothMonster3;
                if (GetMonsterInfo(Settings.HellKnight1, true) == null) return "Cannot start server without mob: " + Settings.HellKnight1;
                if (GetMonsterInfo(Settings.HellKnight2, true) == null) return "Cannot start server without mob: " + Settings.HellKnight2;
                if (GetMonsterInfo(Settings.HellKnight3, true) == null) return "Cannot start server without mob: " + Settings.HellKnight3;
                if (GetMonsterInfo(Settings.HellKnight4, true) == null) return "Cannot start server without mob: " + Settings.HellKnight4;
                if (GetMonsterInfo(Settings.HellBomb1, true) == null) return "Cannot start server without mob: " + Settings.HellBomb1;
                if (GetMonsterInfo(Settings.HellBomb2, true) == null) return "Cannot start server without mob: " + Settings.HellBomb2;
                if (GetMonsterInfo(Settings.HellBomb3, true) == null) return "Cannot start server without mob: " + Settings.HellBomb3;
                if (GetMonsterInfo(Settings.WhiteSnake, true) == null) return "Cannot start server without mob: " + Settings.WhiteSnake;
                if (GetMonsterInfo(Settings.AngelName, true) == null) return "Cannot start server without mob: " + Settings.AngelName;
                if (GetMonsterInfo(Settings.BombSpiderName, true) == null) return "Cannot start server without mob: " + Settings.BombSpiderName;
                if (GetMonsterInfo(Settings.CloneName, true) == null) return "Cannot start server without mob: " + Settings.CloneName;
                if (GetMonsterInfo(Settings.AssassinCloneName, true) == null) return "Cannot start server without mob: " + Settings.AssassinCloneName;
                if (GetMonsterInfo(Settings.VampireName, true) == null) return "Cannot start server without mob: " + Settings.VampireName;
                if (GetMonsterInfo(Settings.ToadName, true) == null) return "Cannot start server without mob: " + Settings.ToadName;
                if (GetMonsterInfo(Settings.SnakeTotemName, true) == null) return "Cannot start server without mob: " + Settings.SnakeTotemName;
                if (GetMonsterInfo(Settings.FishingMonster, true) == null) return "Cannot start server without mob: " + Settings.FishingMonster;

                if (GetItemInfo(Settings.RefineOreName) == null) return "Cannot start server without item: " + Settings.RefineOreName;
            }

            //add intelligent creature checks?

            return "true";
        }

        private void WorkLoop() {
            try {
                Time = Stopwatch.ElapsedMilliseconds;

                var conTime = Time;
                var saveTime = Time + Settings.SaveDelay * Settings.Minute;
                var userTime = Time + Settings.Minute * 5;
                var SpawnTime = Time;
                var processTime = Time + 1000;
                var StartTime = Time;

                var processCount = 0;
                var processRealCount = 0;

                LinkedListNode<MapObject> current = null;

                if (Settings.Multithreaded) {
                    for (var j = 0; j < MobThreads.Length; j++) {
                        MobThreads[j] = new MobThread();
                        MobThreads[j].Id = j;
                    }
                }

                StartEnvir();
                var canstartserver = CanStartEnvir();
                if (canstartserver != "true") {
                    MessageQueue.Enqueue(canstartserver);
                    StopEnvir();
                    _thread = null;
                    Stop();
                    return;
                }

                if (Settings.Multithreaded) {
                    for (var j = 0; j < MobThreads.Length; j++) {
                        var Info = MobThreads[j];
                        if (j <= 0) continue;
                        MobThreading[j] = new Thread(() => ThreadLoop(Info)) {
                            IsBackground = true
                        };
                        MobThreading[j].Start();
                    }
                }

                StartNetwork();
                if (Settings.StartHTTPService) {
                    _http = new HttpServer();
                    _http.Start();
                }
                try {
                    while (Running) {
                        Time = Stopwatch.ElapsedMilliseconds;

                        if (Time >= processTime) {
                            LastCount = processCount;
                            LastRealCount = processRealCount;
                            processCount = 0;
                            processRealCount = 0;
                            processTime = Time + 1000;
                        }


                        if (conTime != Time) {
                            conTime = Time;

                            AdjustLights();

                            lock (Connections) {
                                for (var i = Connections.Count - 1; i >= 0; i--) {
                                    Connections[i].Process();
                                }
                            }

                            lock (StatusConnections) {
                                for (var i = StatusConnections.Count - 1; i >= 0; i--) {
                                    StatusConnections[i].Process();
                                }
                            }
                        }


                        if (current == null)
                            current = Objects.First;

                        if (current == Objects.First) {
                            LastRunTime = Time - StartTime;
                            StartTime = Time;
                        }

                        if (Settings.Multithreaded) {
                            for (var j = 1; j < MobThreads.Length; j++) {
                                var Info = MobThreads[j];

                                if (!Info.Stop) continue;
                                Info.EndTime = Time + 10;
                                Info.Stop = false;
                            }
                            lock (_locker) {
                                Monitor.PulseAll(_locker); // changing a blocking condition. (this makes the threads wake up!)
                            }
                            //run the first loop in the main thread so the main thread automaticaly 'halts' untill the other threads are finished
                            ThreadLoop(MobThreads[0]);
                        }

                        var TheEnd = false;
                        var Start = Stopwatch.ElapsedMilliseconds;
                        while (!TheEnd && Stopwatch.ElapsedMilliseconds - Start < 20) {
                            if (current == null) {
                                TheEnd = true;
                                break;
                            }

                            var next = current.Next;
                            if (!Settings.Multithreaded || current.Value.Race != ObjectType.Monster || current.Value.Master != null) {
                                if (Time > current.Value.OperateTime) {

                                    current.Value.Process();
                                    current.Value.SetOperateTime();
                                }
                                processCount++;
                            }
                            current = next;
                        }

                        for (var i = 0; i < MapList.Count; i++)
                            MapList[i].Process();

                        DragonSystem?.Process();

                        Process();

                        if (Time >= saveTime) {
                            saveTime = Time + Settings.SaveDelay * Settings.Minute;
                            BeginSaveAccounts();
                            SaveGuilds();
                            SaveGoods();
                            SaveConquests();
                        }

                        if (Time >= userTime) {
                            userTime = Time + Settings.Minute * 5;
                            Broadcast(new S.Chat {
                                Message = string.Format(GameLanguage.OnlinePlayers, Players.Count),
                                Type = ChatType.Hint
                            });
                        }

                        if (Time >= SpawnTime) {
                            SpawnTime = Time + Settings.Second * 10; //technicaly this limits the respawn tick code to a minimum of 10 second each but lets assume it's not meant to be this accurate
                            Main.RespawnTick.Process();
                        }

                        //   if (Players.Count == 0) Thread.Sleep(1);
                        //   GC.Collect();


                    }

                } catch (Exception ex) {
                    MessageQueue.Enqueue(ex);

                    lock (Connections) {
                        for (var i = Connections.Count - 1; i >= 0; i--)
                            Connections[i].SendDisconnect(3);
                    }

                    // Get stack trace for the exception with source file information
                    var st = new StackTrace(ex, true);
                    // Get the top stack frame
                    var frame = st.GetFrame(0);
                    // Get the line number from the stack frame
                    var line = frame.GetFileLineNumber();

                    File.AppendAllText(Path.Combine(Settings.ErrorPath, "Error.txt"),
                        $"[{Now}] {ex} at line {line}{Environment.NewLine}");
                }

                StopNetwork();
                StopEnvir();
                SaveAccounts();
                SaveGuilds(true);
                SaveConquests(true);

            } catch (Exception ex) {
                // Get stack trace for the exception with source file information
                var st = new StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                MessageQueue.Enqueue("[outer workloop error]" + ex);
                File.AppendAllText(Path.Combine(Settings.ErrorPath, "Error.txt"),
                    $"[{Now}] {ex} at line {line}{Environment.NewLine}");
            }
            _thread = null;

        }

        private void ThreadLoop(MobThread Info) {
            Info.Stop = false;
            var starttime = Time;
            try {

                var stopping = false;
                if (Info._current == null)
                    Info._current = Info.ObjectsList.First;
                stopping = Info._current == null;
                //while (stopping == false)
                while (Running) {
                    if (Info._current == null)
                        Info._current = Info.ObjectsList.First;
                    else {
                        var next = Info._current.Next;

                        //if we reach the end of our list > go back to the top (since we are running threaded, we dont want the system to sit there for xxms doing nothing)
                        if (Info._current == Info.ObjectsList.Last) {
                            next = Info.ObjectsList.First;
                            Info.LastRunTime = (Info.LastRunTime + (Time - Info.StartTime)) / 2;
                            //Info.LastRunTime = (Time - Info.StartTime) /*> 0 ? (Time - Info.StartTime) : Info.LastRunTime */;
                            Info.StartTime = Time;
                        }
                        if (Time > Info._current.Value.OperateTime) {
                            if (Info._current.Value.Master == null) //since we are running multithreaded, dont allow pets to be processed (unless you constantly move pets into their map appropriate thead)
                            {
                                Info._current.Value.Process();
                                Info._current.Value.SetOperateTime();
                            }
                        }
                        Info._current = next;
                    }
                    //if it's the main thread > make it loop till the subthreads are done, else make it stop after 'endtime'
                    if (Info.Id == 0) {
                        stopping = true;
                        for (var x = 1; x < MobThreads.Length; x++)
                            if (MobThreads[x].Stop == false)
                                stopping = false;
                        if (!stopping) continue;
                        Info.Stop = stopping;
                        return;
                    }

                    if (Stopwatch.ElapsedMilliseconds <= Info.EndTime || !Running) continue;
                    Info.Stop = true;
                    lock (_locker) {
                        while (Info.Stop) Monitor.Wait(_locker);
                    }
                }
            } catch (Exception ex) {
                if (ex is ThreadInterruptedException) return;
                MessageQueue.Enqueue(ex);

                File.AppendAllText(Path.Combine(Settings.ErrorPath, "Error.txt"),
                    $"[{Now}] {ex}{Environment.NewLine}");
            }
            //Info.Stop = true;
        }

        private void AdjustLights() {
            var oldLights = Lights;

            var hours = Now.Hour * 2 % 24;
            if (hours == 6 || hours == 7)
                Lights = LightSetting.Dawn;
            else if (hours >= 8 && hours <= 15)
                Lights = LightSetting.Day;
            else if (hours == 16 || hours == 17)
                Lights = LightSetting.Evening;
            else
                Lights = LightSetting.Night;

            if (oldLights == Lights) return;

            Broadcast(new S.TimeOfDay {
                Lights = Lights
            });
        }

        public void Process() {
            //if we get to a new day : reset daily's
            if (Now.Day != DailyTime) {
                DailyTime = Now.Day;
                ProcessNewDay();
            }

            if (Time >= warTime) {
                for (var i = GuildsAtWar.Count - 1; i >= 0; i--) {
                    GuildsAtWar[i].TimeRemaining -= Settings.Minute;

                    if (GuildsAtWar[i].TimeRemaining >= 0) continue;
                    GuildsAtWar[i].EndWar();
                    GuildsAtWar.RemoveAt(i);
                }

                warTime = Time + Settings.Minute;
            }

            if (Time >= mailTime) {
                for (var i = Mail.Count - 1; i >= 0; i--) {
                    var mail = Mail[i];

                    if (mail.Receive()) {
                        //collected mail ok
                    }
                }

                mailTime = Time + Settings.Minute * 1;
            }

            if (Time >= guildTime) {
                guildTime = Time + Settings.Minute;
                for (var i = 0; i < GuildList.Count; i++) {
                    GuildList[i].Process();
                }
            }

            if (Time >= conquestTime) {
                conquestTime = Time + Settings.Second * 10;
                for (var i = 0; i < Conquests.Count; i++)
                    Conquests[i].Process();
            }

            if (Time < rentalItemsTime) return;
            rentalItemsTime = Time + Settings.Minute * 5;
            ProcessRentedItems();

        }

        public void Broadcast(Packet p) {
            for (var i = 0; i < Players.Count; i++) Players[i].Enqueue(p);
        }

        public void RequiresBaseStatUpdate() {
            for (var i = 0; i < Players.Count; i++) Players[i].HasUpdatedBaseStats = false;
        }

        public void SaveDB() {
            using (var stream = File.Create(DatabasePath))
            using (var writer = new BinaryWriter(stream)) {
                writer.Write(Version);
                writer.Write(CustomVersion);
                writer.Write(MapIndex);
                writer.Write(ItemIndex);
                writer.Write(MonsterIndex);
                writer.Write(NPCIndex);
                writer.Write(QuestIndex);
                writer.Write(GameshopIndex);
                writer.Write(ConquestIndex);
                writer.Write(RespawnIndex);

                writer.Write(MapInfoList.Count);
                for (var i = 0; i < MapInfoList.Count; i++)
                    MapInfoList[i].Save(writer);

                writer.Write(ItemInfoList.Count);
                for (var i = 0; i < ItemInfoList.Count; i++)
                    ItemInfoList[i].Save(writer);

                writer.Write(MonsterInfoList.Count);
                for (var i = 0; i < MonsterInfoList.Count; i++)
                    MonsterInfoList[i].Save(writer);

                writer.Write(NPCInfoList.Count);
                for (var i = 0; i < NPCInfoList.Count; i++)
                    NPCInfoList[i].Save(writer);

                writer.Write(QuestInfoList.Count);
                for (var i = 0; i < QuestInfoList.Count; i++)
                    QuestInfoList[i].Save(writer);

                DragonInfo.Save(writer);
                writer.Write(MagicEnvir.MagicInfoList.Count);
                for (var i = 0; i < MagicEnvir.MagicInfoList.Count; i++)
                    MagicEnvir.MagicInfoList[i].Save(writer);

                writer.Write(GameShopList.Count);
                for (var i = 0; i < GameShopList.Count; i++)
                    GameShopList[i].Save(writer);

                writer.Write(ConquestInfos.Count);
                for (var i = 0; i < ConquestInfos.Count; i++)
                    ConquestInfos[i].Save(writer);

                RespawnTick.Save(writer);
            }
        }
        public void SaveAccounts() {
            while (Saving)
                Thread.Sleep(1);

            try {
                using (var stream = File.Create(AccountPath + "n"))
                    SaveAccounts(stream);
                if (File.Exists(AccountPath))
                    File.Move(AccountPath, AccountPath + "o");
                File.Move(AccountPath + "n", AccountPath);
                if (File.Exists(AccountPath + "o"))
                    File.Delete(AccountPath + "o");
            } catch (Exception ex) {
                MessageQueue.Enqueue(ex);
            }
        }

        private void SaveAccounts(Stream stream) {
            using (var writer = new BinaryWriter(stream)) {
                writer.Write(Version);
                writer.Write(CustomVersion);
                writer.Write(NextAccountID);
                writer.Write(NextCharacterID);
                writer.Write(NextUserItemID);
                writer.Write(GuildList.Count);
                writer.Write(NextGuildID);
                writer.Write(AccountList.Count);
                for (var i = 0; i < AccountList.Count; i++)
                    AccountList[i].Save(writer);

                writer.Write(NextAuctionID);
                writer.Write(Auctions.Count);
                foreach (var auction in Auctions)
                    auction.Save(writer);

                writer.Write(NextMailID);
                writer.Write(Mail.Count);
                foreach (var mail in Mail)
                    mail.Save(writer);

                writer.Write(GameshopLog.Count);
                foreach (var item in GameshopLog) {
                    writer.Write(item.Key);
                    writer.Write(item.Value);
                }

                writer.Write(SavedSpawns.Count);
                foreach (var Spawn in SavedSpawns) {
                    var Save = new RespawnSave {
                        RespawnIndex = Spawn.Info.RespawnIndex,
                        NextSpawnTick = Spawn.NextSpawnTick,
                        Spawned = Spawn.Count >= Spawn.Info.Count * spawnmultiplyer
                    };
                    Save.save(writer);
                }
            }
        }

        private void SaveGuilds(bool forced = false) {
            if (!Directory.Exists(Settings.GuildPath)) Directory.CreateDirectory(Settings.GuildPath);
            for (var i = 0; i < GuildList.Count; i++) {
                if (GuildList[i].NeedSave || forced) {
                    GuildList[i].NeedSave = false;
                    var mStream = new MemoryStream();
                    var writer = new BinaryWriter(mStream);
                    GuildList[i].Save(writer);
                    var fStream = new FileStream(Path.Combine(Settings.GuildPath, i + ".mgdn"), FileMode.Create);
                    var data = mStream.ToArray();
                    fStream.BeginWrite(data, 0, data.Length, EndSaveGuildsAsync, fStream);
                }
            }
        }
        private void EndSaveGuildsAsync(IAsyncResult result) {
            var fStream = result.AsyncState as FileStream;
            try {
                if (fStream == null) return;
                var oldfilename = fStream.Name.Substring(0, fStream.Name.Length - 1);
                var newfilename = fStream.Name;
                fStream.EndWrite(result);
                fStream.Dispose();
                if (File.Exists(oldfilename))
                    File.Move(oldfilename, oldfilename + "o");
                File.Move(newfilename, oldfilename);
                if (File.Exists(oldfilename + "o"))
                    File.Delete(oldfilename + "o");
            } catch (Exception) {
            }
        }

        private void SaveGoods(bool forced = false) {
            if (!Directory.Exists(Settings.GoodsPath)) Directory.CreateDirectory(Settings.GoodsPath);

            for (var i = 0; i < MapList.Count; i++) {
                var map = MapList[i];

                if (map.NPCs.Count < 1) continue;

                for (var j = 0; j < map.NPCs.Count; j++) {
                    var npc = map.NPCs[j];

                    if (forced) {
                        npc.ProcessGoods(forced);
                    }

                    if (!npc.NeedSave) continue;

                    var path = Path.Combine(Settings.GoodsPath, npc.Info.Index + ".msdn");

                    var mStream = new MemoryStream();
                    var writer = new BinaryWriter(mStream);
                    var Temp = 9999;
                    writer.Write(Temp);
                    writer.Write(Version);
                    writer.Write(CustomVersion);
                    writer.Write(npc.UsedGoods.Count);

                    for (var k = 0; k < npc.UsedGoods.Count; k++) {
                        npc.UsedGoods[k].Save(writer);
                    }

                    var fStream = new FileStream(path, FileMode.Create);
                    var data = mStream.ToArray();
                    fStream.BeginWrite(data, 0, data.Length, EndSaveGoodsAsync, fStream);
                }
            }
        }
        private void EndSaveGoodsAsync(IAsyncResult result) {
            try {
                var fStream = result.AsyncState as FileStream;
                if (fStream == null) return;
                var oldfilename = fStream.Name.Substring(0, fStream.Name.Length - 1);
                var newfilename = fStream.Name;
                fStream.EndWrite(result);
                fStream.Dispose();
                if (File.Exists(oldfilename))
                    File.Move(oldfilename, oldfilename + "o");
                File.Move(newfilename, oldfilename);
                if (File.Exists(oldfilename + "o"))
                    File.Delete(oldfilename + "o");
            } catch (Exception) {
            }
        }

        private void SaveConquests(bool forced = false) {
            if (!Directory.Exists(Settings.ConquestsPath)) Directory.CreateDirectory(Settings.ConquestsPath);
            for (var i = 0; i < Conquests.Count; i++) {
                if (!Conquests[i].NeedSave && !forced) continue;
                Conquests[i].NeedSave = false;
                var mStream = new MemoryStream();
                var writer = new BinaryWriter(mStream);
                Conquests[i].Save(writer);
                var fStream = new FileStream(Path.Combine(Settings.ConquestsPath, Conquests[i].Info.Index + ".mcdn"), FileMode.Create);
                var data = mStream.ToArray();
                fStream.BeginWrite(data, 0, data.Length, EndSaveConquestsAsync, fStream);
            }
        }
        private void EndSaveConquestsAsync(IAsyncResult result) {
            var fStream = result.AsyncState as FileStream;
            try {
                if (fStream == null) return;
                var oldfilename = fStream.Name.Substring(0, fStream.Name.Length - 1);
                var newfilename = fStream.Name;
                fStream.EndWrite(result);
                fStream.Dispose();
                if (File.Exists(oldfilename))
                    File.Move(oldfilename, oldfilename + "o");
                File.Move(newfilename, oldfilename);
                if (File.Exists(oldfilename + "o"))
                    File.Delete(oldfilename + "o");
            } catch (Exception) {

            }
        }

        public void BeginSaveAccounts() {
            if (Saving) return;

            Saving = true;


            using (var mStream = new MemoryStream()) {
                if (File.Exists(AccountPath)) {
                    if (!Directory.Exists(BackUpPath)) Directory.CreateDirectory(BackUpPath);
                    var fileName =
                        $"Accounts {Now.Year:0000}-{Now.Month:00}-{Now.Day:00} {Now.Hour:00}-{Now.Minute:00}-{Now.Second:00}.bak";
                    if (File.Exists(Path.Combine(BackUpPath, fileName))) File.Delete(Path.Combine(BackUpPath, fileName));
                    File.Move(AccountPath, Path.Combine(BackUpPath, fileName));
                }

                SaveAccounts(mStream);
                var fStream = new FileStream(AccountPath + "n", FileMode.Create);

                var data = mStream.ToArray();
                fStream.BeginWrite(data, 0, data.Length, EndSaveAccounts, fStream);
            }

        }
        private void EndSaveAccounts(IAsyncResult result) {
            var fStream = result.AsyncState as FileStream;
            try {
                if (fStream != null) {
                    var oldfilename = fStream.Name.Substring(0, fStream.Name.Length - 1);
                    var newfilename = fStream.Name;
                    fStream.EndWrite(result);
                    fStream.Dispose();
                    if (File.Exists(oldfilename))
                        File.Move(oldfilename, oldfilename + "o");
                    File.Move(newfilename, oldfilename);
                    if (File.Exists(oldfilename + "o"))
                        File.Delete(oldfilename + "o");
                }
            } catch (Exception) {
            }

            Saving = false;
        }

        public void LoadDB() {
            lock (LoadLock) {
                if (!File.Exists(DatabasePath))
                    SaveDB();

                using (var stream = File.OpenRead(DatabasePath))
                using (var reader = new BinaryReader(stream)) {
                    LoadVersion = reader.ReadInt32();
                    if (LoadVersion > 57)
                        LoadCustomVersion = reader.ReadInt32();
                    MapIndex = reader.ReadInt32();
                    ItemIndex = reader.ReadInt32();
                    MonsterIndex = reader.ReadInt32();

                    if (LoadVersion > 33) {
                        NPCIndex = reader.ReadInt32();
                        QuestIndex = reader.ReadInt32();
                    }
                    if (LoadVersion >= 63) {
                        GameshopIndex = reader.ReadInt32();
                    }

                    if (LoadVersion >= 66) {
                        ConquestIndex = reader.ReadInt32();
                    }

                    if (LoadVersion >= 68)
                        RespawnIndex = reader.ReadInt32();


                    var count = reader.ReadInt32();
                    MapInfoList.Clear();
                    for (var i = 0; i < count; i++)
                        MapInfoList.Add(new MapInfo(reader));

                    count = reader.ReadInt32();
                    ItemInfoList.Clear();
                    for (var i = 0; i < count; i++) {
                        ItemInfoList.Add(new ItemInfo(reader, LoadVersion, LoadCustomVersion));
                        if (ItemInfoList[i] != null && ItemInfoList[i].RandomStatsId < Settings.RandomItemStatsList.Count) {
                            ItemInfoList[i].RandomStats = Settings.RandomItemStatsList[ItemInfoList[i].RandomStatsId];
                        }
                    }
                    count = reader.ReadInt32();
                    MonsterInfoList.Clear();
                    for (var i = 0; i < count; i++)
                        MonsterInfoList.Add(new MonsterInfo(reader));

                    if (LoadVersion > 33) {
                        count = reader.ReadInt32();
                        NPCInfoList.Clear();
                        for (var i = 0; i < count; i++)
                            NPCInfoList.Add(new NPCInfo(reader));

                        count = reader.ReadInt32();
                        QuestInfoList.Clear();
                        for (var i = 0; i < count; i++)
                            QuestInfoList.Add(new QuestInfo(reader));
                    }

                    DragonInfo = LoadVersion >= 11 ? new DragonInfo(reader) : new DragonInfo();
                    if (LoadVersion >= 58) {
                        count = reader.ReadInt32();
                        for (var i = 0; i < count; i++) {
                            var m = new MagicInfo(reader, LoadVersion, LoadCustomVersion);
                            if (!MagicEnvir.MagicExists(m.Spell))
                                MagicEnvir.MagicInfoList.Add(m);
                        }
                    }
                    MagicEnvir.FillMagicInfoList();
                    if (LoadVersion <= 70)
                        MagicEnvir.UpdateMagicInfo();

                    if (LoadVersion >= 63) {
                        count = reader.ReadInt32();
                        GameShopList.Clear();
                        for (var i = 0; i < count; i++) {
                            var item = new GameShopItem(reader, LoadVersion, LoadCustomVersion);
                            if (Main.BindGameShop(item)) {
                                GameShopList.Add(item);
                            }
                        }
                    }

                    if (LoadVersion >= 66) {
                        ConquestInfos.Clear();
                        count = reader.ReadInt32();
                        for (var i = 0; i < count; i++) {
                            ConquestInfos.Add(new ConquestInfo(reader));
                        }
                    }

                    if (LoadVersion > 67)
                        RespawnTick = new RespawnTimer(reader);

                }

                Settings.LinkGuildCreationItems(ItemInfoList);
            }

        }

        public void LoadAccounts() {
            //reset ranking
            for (var i = 0; i < _rankClass.Count(); i++) {
                if (_rankClass[i] != null)
                    _rankClass[i].Clear();
                else
                    _rankClass[i] = new List<Rank_Character_Info>();
            }
            RankTop.Clear();
            for (var i = 0; i < RankBottomLevel.Count(); i++) {
                RankBottomLevel[i] = 0;
            }


            lock (LoadLock) {
                if (!File.Exists(AccountPath))
                    SaveAccounts();

                using (var stream = File.OpenRead(AccountPath))
                using (var reader = new BinaryReader(stream)) {
                    LoadVersion = reader.ReadInt32();
                    if (LoadVersion > 57) LoadCustomVersion = reader.ReadInt32();
                    NextAccountID = reader.ReadInt32();
                    NextCharacterID = reader.ReadInt32();
                    NextUserItemID = reader.ReadUInt64();

                    if (LoadVersion > 27) {
                        GuildCount = reader.ReadInt32();
                        NextGuildID = reader.ReadInt32();
                    }

                    var count = reader.ReadInt32();
                    AccountList.Clear();
                    CharacterList.Clear();
                    for (var i = 0; i < count; i++) {
                        AccountList.Add(new AccountInfo(reader));
                        CharacterList.AddRange(AccountList[i].Characters);
                    }

                    if (LoadVersion < 7) return;

                    foreach (var auction in Auctions)
                        auction.CharacterInfo.AccountInfo.Auctions.Remove(auction);
                    Auctions.Clear();

                    if (LoadVersion >= 8)
                        NextAuctionID = reader.ReadUInt64();

                    count = reader.ReadInt32();
                    for (var i = 0; i < count; i++) {
                        var auction = new AuctionInfo(reader, LoadVersion, LoadCustomVersion);

                        if (!BindItem(auction.Item) || !BindCharacter(auction)) continue;

                        Auctions.AddLast(auction);
                        auction.CharacterInfo.AccountInfo.Auctions.AddLast(auction);
                    }

                    if (LoadVersion == 7) {
                        foreach (var auction in Auctions) {
                            if (auction.Sold && auction.Expired) auction.Expired = false;

                            auction.AuctionID = ++NextAuctionID;
                        }
                    }

                    if (LoadVersion > 43) {
                        NextMailID = reader.ReadUInt64();

                        Mail.Clear();

                        count = reader.ReadInt32();
                        for (var i = 0; i < count; i++) {
                            Mail.Add(new MailInfo(reader, LoadVersion, LoadCustomVersion));
                        }
                    }

                    if (LoadVersion >= 63) {
                        var logCount = reader.ReadInt32();
                        for (var i = 0; i < logCount; i++) {
                            GameshopLog.Add(reader.ReadInt32(), reader.ReadInt32());
                        }

                        if (ResetGS) ClearGameshopLog();
                    }

                    if (LoadVersion < 68) return;
                    {
                        var SaveCount = reader.ReadInt32();
                        for (var i = 0; i < SaveCount; i++) {
                            var Saved = new RespawnSave(reader);
                            foreach (var Respawn in SavedSpawns) {
                                if (Respawn.Info.RespawnIndex != Saved.RespawnIndex) continue;
                                Respawn.NextSpawnTick = Saved.NextSpawnTick;
                                if (!Saved.Spawned || Respawn.Info.Count * spawnmultiplyer <= Respawn.Count)
                                    continue;
                                var mobcount = Respawn.Info.Count * spawnmultiplyer - Respawn.Count;
                                for (var j = 0; j < mobcount; j++) {
                                    Respawn.Spawn();
                                }
                            }

                        }
                    }
                }
            }
        }

        public void LoadGuilds() {
            lock (LoadLock) {
                var count = 0;

                GuildList.Clear();

                for (var i = 0; i < GuildCount; i++) {
                    GuildObject newGuild;
                    if (!File.Exists(Path.Combine(Settings.GuildPath, i + ".mgd"))) continue;
                    using (var stream = File.OpenRead(Path.Combine(Settings.GuildPath, i + ".mgd")))
                    using (var reader = new BinaryReader(stream))
                        newGuild = new GuildObject(reader);

                    //if (!newGuild.Ranks.Any(a => (byte)a.Options == 255)) continue;
                    //if (GuildList.Any(e => e.Name == newGuild.Name)) continue;
                    GuildList.Add(newGuild);

                    count++;
                }

                if (count != GuildCount) GuildCount = count;
            }
        }

        public void LoadFishingDrops() {
            _fishingDrops.Clear();

            for (byte i = 0; i <= 19; i++) {
                var path = Path.Combine(Settings.DropPath, Settings.FishingDropFilename + ".txt");

                path = path.Replace("00", i.ToString("D2"));

                if (!File.Exists(path) && i < 2) {
                    var newfile = File.Create(path);
                    newfile.Close();
                }

                if (!File.Exists(path)) continue;

                var lines = File.ReadAllLines(path);

                for (var j = 0; j < lines.Length; j++) {
                    if (lines[j].StartsWith(";") || string.IsNullOrWhiteSpace(lines[j])) continue;

                    var drop = DropInfo.FromLine(lines[j]);
                    if (drop == null) {
                        MessageQueue.Enqueue($"Could not load fishing drop: {lines[j]}");
                        continue;
                    }

                    drop.Type = i;

                    _fishingDrops.Add(drop);
                }

                _fishingDrops.Sort((drop1, drop2) => {
                    if (drop1.Chance > 0 && drop2.Chance == 0)
                        return 1;
                    if (drop1.Chance == 0 && drop2.Chance > 0)
                        return -1;

                    return drop1.Item.Type.CompareTo(drop2.Item.Type);
                });
            }
        }

        public void LoadAwakeningMaterials() {
            _awakeningDrops.Clear();

            var path = Path.Combine(Settings.DropPath, Settings.AwakeningDropFilename + ".txt");

            if (!File.Exists(path)) {
                var newfile = File.Create(path);
                newfile.Close();

            }

            var lines = File.ReadAllLines(path);

            for (var i = 0; i < lines.Length; i++) {
                if (lines[i].StartsWith(";") || string.IsNullOrWhiteSpace(lines[i])) continue;

                var drop = DropInfo.FromLine(lines[i]);
                if (drop == null) {
                    MessageQueue.Enqueue($"Could not load Awakening drop: {lines[i]}");
                    continue;
                }

                _awakeningDrops.Add(drop);
            }

            _awakeningDrops.Sort((drop1, drop2) => {
                if (drop1.Chance > 0 && drop2.Chance == 0)
                    return 1;
                if (drop1.Chance == 0 && drop2.Chance > 0)
                    return -1;

                return drop1.Item.Type.CompareTo(drop2.Item.Type);
            });
        }

        public void LoadStrongBoxDrops() {
            _strongboxDrops.Clear();

            var path = Path.Combine(Settings.DropPath, Settings.StrongboxDropFilename + ".txt");

            if (!File.Exists(path)) {
                var newfile = File.Create(path);
                newfile.Close();
            }

            var lines = File.ReadAllLines(path);

            for (var i = 0; i < lines.Length; i++) {
                if (lines[i].StartsWith(";") || string.IsNullOrWhiteSpace(lines[i])) continue;

                var drop = DropInfo.FromLine(lines[i]);
                if (drop == null) {
                    MessageQueue.Enqueue($"Could not load strongbox drop: {lines[i]}");
                    continue;
                }

                _strongboxDrops.Add(drop);
            }

            _strongboxDrops.Sort((drop1, drop2) => {
                if (drop1.Chance > 0 && drop2.Chance == 0)
                    return 1;
                if (drop1.Chance == 0 && drop2.Chance > 0)
                    return -1;

                return drop1.Item.Type.CompareTo(drop2.Item.Type);
            });
        }

        public void LoadBlackStoneDrops() {
            _blackstoneDrops.Clear();

            var path = Path.Combine(Settings.DropPath, Settings.BlackstoneDropFilename + ".txt");

            if (!File.Exists(path)) {
                var newfile = File.Create(path);
                newfile.Close();

            }

            var lines = File.ReadAllLines(path);

            for (var i = 0; i < lines.Length; i++) {
                if (lines[i].StartsWith(";") || string.IsNullOrWhiteSpace(lines[i])) continue;

                var drop = DropInfo.FromLine(lines[i]);
                if (drop == null) {
                    MessageQueue.Enqueue($"Could not load blackstone drop: {lines[i]}");
                    continue;
                }

                _blackstoneDrops.Add(drop);
            }

            _blackstoneDrops.Sort((drop1, drop2) => {
                if (drop1.Chance > 0 && drop2.Chance == 0)
                    return 1;
                if (drop1.Chance == 0 && drop2.Chance > 0)
                    return -1;

                return drop1.Item.Type.CompareTo(drop2.Item.Type);
            });
        }

        public void LoadConquests() {
            lock (LoadLock) {
                var count = 0;

                Conquests.Clear();

                Map tempMap;
                ConquestArcherObject tempArcher;
                ConquestGateObject tempGate;
                ConquestWallObject tempWall;
                ConquestSiegeObject tempSiege;

                for (var i = 0; i < ConquestInfos.Count; i++) {
                    ConquestObject newConquest;
                    tempMap = GetMap(ConquestInfos[i].MapIndex);

                    if (tempMap == null) continue;

                    if (File.Exists(Path.Combine(Settings.ConquestsPath, ConquestInfos[i].Index + ".mcd"))) {
                        using (var stream = File.OpenRead(Path.Combine(Settings.ConquestsPath, ConquestInfos[i].Index + ".mcd")))
                        using (var reader = new BinaryReader(stream))
                            newConquest = new ConquestObject(reader) {
                                Info = ConquestInfos[i],
                                ConquestMap = tempMap
                            };

                        for (var k = 0; k < GuildList.Count; k++) {
                            if (newConquest.Owner != GuildList[k].Guildindex) continue;
                            newConquest.Guild = GuildList[k];
                            GuildList[k].Conquest = newConquest;
                        }

                        Conquests.Add(newConquest);
                        tempMap.Conquest.Add(newConquest);
                        count++;
                    } else {
                        newConquest = new ConquestObject {
                            Info = ConquestInfos[i],
                            NeedSave = true,
                            ConquestMap = tempMap
                        };

                        Conquests.Add(newConquest);
                        tempMap.Conquest.Add(newConquest);
                    }

                    //Bind Info to Saved Archer objects or create new objects
                    for (var j = 0; j < ConquestInfos[i].ConquestGuards.Count; j++) {
                        tempArcher = newConquest.ArcherList.FirstOrDefault(x => x.Index == ConquestInfos[i].ConquestGuards[j].Index);

                        if (tempArcher != null) {
                            tempArcher.Info = ConquestInfos[i].ConquestGuards[j];
                            tempArcher.Conquest = newConquest;
                        } else {
                            newConquest.ArcherList.Add(new ConquestArcherObject {
                                Info = ConquestInfos[i].ConquestGuards[j],
                                Alive = true,
                                Index = ConquestInfos[i].ConquestGuards[j].Index,
                                Conquest = newConquest
                            });
                        }
                    }

                    //Remove archers that have been removed from DB
                    for (var j = 0; j < newConquest.ArcherList.Count; j++) {
                        if (newConquest.ArcherList[j].Info == null)
                            newConquest.ArcherList.Remove(newConquest.ArcherList[j]);
                    }

                    //Bind Info to Saved Gate objects or create new objects
                    for (var j = 0; j < ConquestInfos[i].ConquestGates.Count; j++) {
                        tempGate = newConquest.GateList.FirstOrDefault(x => x.Index == ConquestInfos[i].ConquestGates[j].Index);

                        if (tempGate != null) {
                            tempGate.Info = ConquestInfos[i].ConquestGates[j];
                            tempGate.Conquest = newConquest;
                        } else {
                            newConquest.GateList.Add(new ConquestGateObject {
                                Info = ConquestInfos[i].ConquestGates[j],
                                Health = uint.MaxValue,
                                Index = ConquestInfos[i].ConquestGates[j].Index,
                                Conquest = newConquest
                            });
                        }
                    }

                    //Bind Info to Saved Flag objects or create new objects
                    for (var j = 0; j < ConquestInfos[i].ConquestFlags.Count; j++) {
                        newConquest.FlagList.Add(new ConquestFlagObject {
                            Info = ConquestInfos[i].ConquestFlags[j],
                            Index = ConquestInfos[i].ConquestFlags[j].Index,
                            Conquest = newConquest
                        });
                    }

                    //Remove Gates that have been removed from DB
                    for (var j = 0; j < newConquest.GateList.Count; j++) {
                        if (newConquest.GateList[j].Info == null)
                            newConquest.GateList.Remove(newConquest.GateList[j]);
                    }

                    //Bind Info to Saved Wall objects or create new objects
                    for (var j = 0; j < ConquestInfos[i].ConquestWalls.Count; j++) {
                        tempWall = newConquest.WallList.FirstOrDefault(x => x.Index == ConquestInfos[i].ConquestWalls[j].Index);

                        if (tempWall != null) {
                            tempWall.Info = ConquestInfos[i].ConquestWalls[j];
                            tempWall.Conquest = newConquest;
                        } else {
                            newConquest.WallList.Add(new ConquestWallObject {
                                Info = ConquestInfos[i].ConquestWalls[j],
                                Index = ConquestInfos[i].ConquestWalls[j].Index,
                                Health = uint.MaxValue,
                                Conquest = newConquest
                            });
                        }
                    }

                    //Remove Walls that have been removed from DB
                    for (var j = 0; j < newConquest.WallList.Count; j++) {
                        if (newConquest.WallList[j].Info == null)
                            newConquest.WallList.Remove(newConquest.WallList[j]);
                    }


                    //Bind Info to Saved Siege objects or create new objects
                    for (var j = 0; j < ConquestInfos[i].ConquestSieges.Count; j++) {
                        tempSiege = newConquest.SiegeList.FirstOrDefault(x => x.Index == ConquestInfos[i].ConquestSieges[j].Index);

                        if (tempSiege != null) {
                            tempSiege.Info = ConquestInfos[i].ConquestSieges[j];
                            tempSiege.Conquest = newConquest;
                        } else {
                            newConquest.SiegeList.Add(new ConquestSiegeObject {
                                Info = ConquestInfos[i].ConquestSieges[j],
                                Index = ConquestInfos[i].ConquestSieges[j].Index,
                                Health = uint.MaxValue,
                                Conquest = newConquest
                            });
                        }
                    }

                    //Remove Siege that have been removed from DB
                    for (var j = 0; j < newConquest.SiegeList.Count; j++) {
                        if (newConquest.SiegeList[j].Info == null)
                            newConquest.SiegeList.Remove(newConquest.SiegeList[j]);
                    }

                    //Bind Info to Saved Flag objects or create new objects
                    for (var j = 0; j < ConquestInfos[i].ControlPoints.Count; j++) {
                        ConquestFlagObject cp;
                        newConquest.ControlPoints.Add(cp = new ConquestFlagObject {
                            Info = ConquestInfos[i].ControlPoints[j],
                            Index = ConquestInfos[i].ControlPoints[j].Index,
                            Conquest = newConquest
                        }, new Dictionary<GuildObject, int>());

                        cp.Spawn();
                    }


                    newConquest.LoadArchers();
                    newConquest.LoadGates();
                    newConquest.LoadWalls();
                    newConquest.LoadSieges();
                    newConquest.LoadFlags();
                    newConquest.LoadNPCs();
                }
            }
        }

        private bool BindCharacter(AuctionInfo auction) {
            for (var i = 0; i < CharacterList.Count; i++) {
                if (CharacterList[i].Index != auction.CharacterIndex) continue;

                auction.CharacterInfo = CharacterList[i];
                return true;
            }
            return false;

        }

        public void Start() {
            if (Running || _thread != null) return;

            Running = true;

            _thread = new Thread(WorkLoop) {
                IsBackground = true
            };
            _thread.Start();

        }
        public void Stop() {
            Running = false;

            lock (_locker) {
                Monitor.PulseAll(_locker); // changing a blocking condition. (this makes the threads wake up!)
            }

            //simply intterupt all the mob threads if they are running (will give an invisible error on them but fastest way of getting rid of them on shutdowns)
            for (var i = 1; i < MobThreading.Length; i++) {
                if (MobThreads[i] != null)
                    MobThreads[i].EndTime = Time + 9999;
                if (MobThreading[i] != null &&
                    MobThreading[i].ThreadState != System.Threading.ThreadState.Stopped && MobThreading[i].ThreadState != System.Threading.ThreadState.Unstarted) {
                    MobThreading[i].Interrupt();
                }
            }

            _http?.Stop();

            while (_thread != null)
                Thread.Sleep(1);
        }

        public void Reboot() {
            new Thread(() => {
                MessageQueue.Enqueue("Server rebooting...");
                Stop();
                Start();
            }).Start();
        }

        private void StartEnvir() {
            Players.Clear();
            StartPoints.Clear();
            StartItems.Clear();
            MapList.Clear();
            GameshopLog.Clear();
            CustomCommands.Clear();
            MonsterCount = 0;

            LoadDB();

            RecipeInfoList.Clear();
            foreach (var recipe in Directory.GetFiles(Settings.RecipePath, "*.txt")
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .ToArray())
                RecipeInfoList.Add(new RecipeInfo(recipe));

            MessageQueue.Enqueue($"{RecipeInfoList.Count} Recipes loaded.");

            for (var i = 0; i < MapInfoList.Count; i++)
                MapInfoList[i].CreateMap();
            MessageQueue.Enqueue($"{MapInfoList.Count} Maps Loaded.");

            for (var i = 0; i < ItemInfoList.Count; i++) {
                if (ItemInfoList[i].StartItem)
                    StartItems.Add(ItemInfoList[i]);
            }

            for (var i = 0; i < MonsterInfoList.Count; i++)
                MonsterInfoList[i].LoadDrops();

            LoadFishingDrops();
            LoadAwakeningMaterials();
            LoadStrongBoxDrops();
            LoadBlackStoneDrops();
            MessageQueue.Enqueue("Drops Loaded.");

            if (DragonInfo.Enabled) {
                DragonSystem = new Dragon(DragonInfo);
                if (DragonSystem != null) {
                    if (DragonSystem.Load()) DragonSystem.Info.LoadDrops();
                }

                MessageQueue.Enqueue("Dragon Loaded.");
            }

            DefaultNPC = new NPCObject(new NPCInfo() {
                Name = "DefaultNPC",
                FileName = Settings.DefaultNPCFilename,
                IsDefault = true
            });
            MonsterNPC = new NPCObject(new NPCInfo() {
                Name = "MonsterNPC",
                FileName = Settings.MonsterNPCFilename,
                IsDefault = true
            });
            RobotNPC = new NPCObject(new NPCInfo() {
                Name = "RobotNPC",
                FileName = Settings.RobotNPCFilename,
                IsDefault = true,
                IsRobot = true
            });

            MessageQueue.Enqueue("Envir Started.");
        }
        private void StartNetwork() {
            Connections.Clear();

            LoadAccounts();

            LoadGuilds();

            LoadConquests();

            _listener = new TcpListener(IPAddress.Parse(Settings.IPAddress), Settings.Port);
            _listener.Start();
            _listener.BeginAcceptTcpClient(Connection, null);

            if (StatusPortEnabled) {
                _StatusPort = new TcpListener(IPAddress.Parse(Settings.IPAddress), 3000);
                _StatusPort.Start();
                _StatusPort.BeginAcceptTcpClient(StatusConnection, null);
            }
            MessageQueue.Enqueue("Network Started.");

            //FixGuilds();
        }

        private void StopEnvir() {
            SaveGoods(true);

            MapList.Clear();
            StartPoints.Clear();
            StartItems.Clear();
            Objects.Clear();
            Players.Clear();

            CleanUp();

            GC.Collect();

            MessageQueue.Enqueue("Envir Stopped.");
        }
        private void StopNetwork() {
            _listener.Stop();
            lock (Connections) {
                for (var i = Connections.Count - 1; i >= 0; i--)
                    Connections[i].SendDisconnect(0);
            }

            if (StatusPortEnabled) {
                _StatusPort.Stop();
                for (var i = StatusConnections.Count - 1; i >= 0; i--)
                    StatusConnections[i].SendDisconnect();
            }

            var expire = Time + 5000;

            while (Connections.Count != 0 && Stopwatch.ElapsedMilliseconds < expire) {
                Time = Stopwatch.ElapsedMilliseconds;

                for (var i = Connections.Count - 1; i >= 0; i--)
                    Connections[i].Process();

                Thread.Sleep(1);
            }


            Connections.Clear();

            expire = Time + 10000;
            while (StatusConnections.Count != 0 && Stopwatch.ElapsedMilliseconds < expire) {
                Time = Stopwatch.ElapsedMilliseconds;

                for (var i = StatusConnections.Count - 1; i >= 0; i--)
                    StatusConnections[i].Process();

                Thread.Sleep(1);
            }


            StatusConnections.Clear();
            MessageQueue.Enqueue("Network Stopped.");
        }

        private void CleanUp() {
            for (var i = 0; i < CharacterList.Count; i++) {
                var info = CharacterList[i];

                if (info.Deleted) {
                    #region Mentor Cleanup
                    if (info.Mentor > 0) {
                        var Mentor = GetCharacterInfo(info.Mentor);

                        if (Mentor != null) {
                            Mentor.Mentor = 0;
                            Mentor.MentorExp = 0;
                            Mentor.isMentor = false;
                        }

                        info.Mentor = 0;
                        info.MentorExp = 0;
                        info.isMentor = false;
                    }
                    #endregion

                    #region Marriage Cleanup
                    if (info.Married > 0) {
                        var Lover = GetCharacterInfo(info.Married);

                        info.Married = 0;
                        info.MarriedDate = DateTime.Now;

                        Lover.Married = 0;
                        Lover.MarriedDate = DateTime.Now;
                        if (Lover.Equipment[(int)EquipmentSlot.RingL] != null)
                            Lover.Equipment[(int)EquipmentSlot.RingL].WeddingRing = -1;
                    }
                    #endregion

                    if (info.DeleteDate < DateTime.Now.AddDays(-7)) {
                        //delete char from db
                    }
                }

                if (info.Mail.Count > Settings.MailCapacity) {
                    for (var j = info.Mail.Count - 1 - (int)Settings.MailCapacity; j >= 0; j--) {
                        if (info.Mail[j].DateOpened > DateTime.Now && info.Mail[j].Collected && info.Mail[j].Items.Count == 0 && info.Mail[j].Gold == 0) {
                            info.Mail.Remove(info.Mail[j]);
                        }
                    }
                }
            }
        }

        private void Connection(IAsyncResult result) {
            try {
                if (!Running || !_listener.Server.IsBound) return;
            } catch (Exception e) {
                MessageQueue.Enqueue(e.ToString());
            }

            try {
                var tempTcpClient = _listener.EndAcceptTcpClient(result);
                lock (Connections)
                    Connections.Add(new MirConnection(++_sessionID, tempTcpClient));
            } catch (Exception ex) {
                MessageQueue.Enqueue(ex);
            } finally {
                while (Connections.Count >= Settings.MaxUser)
                    Thread.Sleep(1);

                if (Running && _listener.Server.IsBound)
                    _listener.BeginAcceptTcpClient(Connection, null);
            }
        }

        private void StatusConnection(IAsyncResult result) {
            if (!Running || !_StatusPort.Server.IsBound) return;

            try {
                var tempTcpClient = _StatusPort.EndAcceptTcpClient(result);
                lock (StatusConnections)
                    StatusConnections.Add(new MirStatusConnection(tempTcpClient));
            } catch (Exception ex) {
                MessageQueue.Enqueue(ex);
            } finally {
                while (StatusConnections.Count >= 5) //dont allow to many status port connections it's just an abuse thing
                    Thread.Sleep(1);

                if (Running && _StatusPort.Server.IsBound)
                    _StatusPort.BeginAcceptTcpClient(StatusConnection, null);
            }
        }

        public void NewAccount(ClientPackets.NewAccount p, MirConnection c) {
            if (!Settings.AllowNewAccount) {
                c.Enqueue(new ServerPackets.NewAccount {
                    Result = 0
                });
                return;
            }

            if (!AccountIDReg.IsMatch(p.AccountID)) {
                c.Enqueue(new ServerPackets.NewAccount {
                    Result = 1
                });
                return;
            }

            if (!PasswordReg.IsMatch(p.Password)) {
                c.Enqueue(new ServerPackets.NewAccount {
                    Result = 2
                });
                return;
            }
            if (!string.IsNullOrWhiteSpace(p.EMailAddress) && !EMailReg.IsMatch(p.EMailAddress) ||
                p.EMailAddress.Length > 50) {
                c.Enqueue(new ServerPackets.NewAccount {
                    Result = 3
                });
                return;
            }

            if (!string.IsNullOrWhiteSpace(p.UserName) && p.UserName.Length > 20) {
                c.Enqueue(new ServerPackets.NewAccount {
                    Result = 4
                });
                return;
            }

            if (!string.IsNullOrWhiteSpace(p.SecretQuestion) && p.SecretQuestion.Length > 30) {
                c.Enqueue(new ServerPackets.NewAccount {
                    Result = 5
                });
                return;
            }

            if (!string.IsNullOrWhiteSpace(p.SecretAnswer) && p.SecretAnswer.Length > 30) {
                c.Enqueue(new ServerPackets.NewAccount {
                    Result = 6
                });
                return;
            }

            lock (AccountLock) {
                if (AccountExists(p.AccountID)) {
                    c.Enqueue(new ServerPackets.NewAccount {
                        Result = 7
                    });
                    return;
                }

                AccountList.Add(new AccountInfo(p) {
                    Index = ++NextAccountID,
                    CreationIP = c.IPAddress
                });


                c.Enqueue(new ServerPackets.NewAccount {
                    Result = 8
                });
            }
        }

        public int HTTPNewAccount(ClientPackets.NewAccount p, string ip) {
            if (!Settings.AllowNewAccount) {
                return 0;
            }

            if (!AccountIDReg.IsMatch(p.AccountID)) {
                return 1;
            }

            if (!PasswordReg.IsMatch(p.Password)) {
                return 2;
            }
            if (!string.IsNullOrWhiteSpace(p.EMailAddress) && !EMailReg.IsMatch(p.EMailAddress) ||
                p.EMailAddress.Length > 50) {
                return 3;
            }

            if (!string.IsNullOrWhiteSpace(p.UserName) && p.UserName.Length > 20) {
                return 4;
            }

            if (!string.IsNullOrWhiteSpace(p.SecretQuestion) && p.SecretQuestion.Length > 30) {
                return 5;
            }

            if (!string.IsNullOrWhiteSpace(p.SecretAnswer) && p.SecretAnswer.Length > 30) {
                return 6;
            }

            lock (AccountLock) {
                if (AccountExists(p.AccountID)) {
                    return 7;
                }

                AccountList.Add(new AccountInfo(p) {
                    Index = ++NextAccountID,
                    CreationIP = ip
                });
                return 8;
            }
        }

        public void ChangePassword(ClientPackets.ChangePassword p, MirConnection c) {
            if (!Settings.AllowChangePassword) {
                c.Enqueue(new ServerPackets.ChangePassword {
                    Result = 0
                });
                return;
            }

            if (!AccountIDReg.IsMatch(p.AccountID)) {
                c.Enqueue(new ServerPackets.ChangePassword {
                    Result = 1
                });
                return;
            }

            if (!PasswordReg.IsMatch(p.CurrentPassword)) {
                c.Enqueue(new ServerPackets.ChangePassword {
                    Result = 2
                });
                return;
            }

            if (!PasswordReg.IsMatch(p.NewPassword)) {
                c.Enqueue(new ServerPackets.ChangePassword {
                    Result = 3
                });
                return;
            }

            var account = GetAccount(p.AccountID);

            if (account == null) {
                c.Enqueue(new ServerPackets.ChangePassword {
                    Result = 4
                });
                return;
            }

            if (account.Banned) {
                if (account.ExpiryDate > Now) {
                    c.Enqueue(new ServerPackets.ChangePasswordBanned {
                        Reason = account.BanReason,
                        ExpiryDate = account.ExpiryDate
                    });
                    return;
                }
                account.Banned = false;
            }
            account.BanReason = string.Empty;
            account.ExpiryDate = DateTime.MinValue;

            if (string.CompareOrdinal(account.Password, p.CurrentPassword) != 0) {
                c.Enqueue(new ServerPackets.ChangePassword {
                    Result = 5
                });
                return;
            }

            account.Password = p.NewPassword;
            c.Enqueue(new ServerPackets.ChangePassword {
                Result = 6
            });
        }
        public void Login(ClientPackets.Login p, MirConnection c) {
            if (!Settings.AllowLogin) {
                c.Enqueue(new ServerPackets.Login {
                    Result = 0
                });
                return;
            }

            if (!AccountIDReg.IsMatch(p.AccountID)) {
                c.Enqueue(new ServerPackets.Login {
                    Result = 1
                });
                return;
            }

            if (!PasswordReg.IsMatch(p.Password)) {
                c.Enqueue(new ServerPackets.Login {
                    Result = 2
                });
                return;
            }
            var account = GetAccount(p.AccountID);

            if (account == null) {
                c.Enqueue(new ServerPackets.Login {
                    Result = 3
                });
                return;
            }

            if (account.Banned) {
                if (account.ExpiryDate > DateTime.Now) {
                    c.Enqueue(new ServerPackets.LoginBanned {
                        Reason = account.BanReason,
                        ExpiryDate = account.ExpiryDate
                    });
                    return;
                }
                account.Banned = false;
            }
            account.BanReason = string.Empty;
            account.ExpiryDate = DateTime.MinValue;


            if (string.CompareOrdinal(account.Password, p.Password) != 0) {
                if (account.WrongPasswordCount++ >= 5) {
                    account.Banned = true;
                    account.BanReason = "Too many Wrong Login Attempts.";
                    account.ExpiryDate = DateTime.Now.AddMinutes(2);

                    c.Enqueue(new ServerPackets.LoginBanned {
                        Reason = account.BanReason,
                        ExpiryDate = account.ExpiryDate
                    });
                    return;
                }

                c.Enqueue(new ServerPackets.Login {
                    Result = 4
                });
                return;
            }
            account.WrongPasswordCount = 0;

            lock (AccountLock) {
                account.Connection?.SendDisconnect(1);

                account.Connection = c;
            }

            c.Account = account;
            c.Stage = GameStage.Select;

            account.LastDate = Now;
            account.LastIP = c.IPAddress;

            MessageQueue.Enqueue(account.Connection.SessionID + ", " + account.Connection.IPAddress + ", User logged in.");
            c.Enqueue(new ServerPackets.LoginSuccess {
            });
        }

        public void Logout(ClientPackets.Logout p, MirConnection c) {
            c.Account.Connection = null;
            c.Account = null;
            c.Stage = GameStage.Login;
            c.Enqueue(new ServerPackets.LogoutSuccess {
            });
        }

        public void RequestCharacters(MirConnection c) {
            c.Enqueue(new ServerPackets.SelectCharacters {
                Characters = c.Account.GetSelectInfo()
            });
        }

        public int HTTPLogin(string AccountID, string Password) {
            if (!Settings.AllowLogin) {
                return 0;
            }

            if (!AccountIDReg.IsMatch(AccountID)) {
                return 1;
            }

            if (!PasswordReg.IsMatch(Password)) {
                return 2;
            }

            var account = GetAccount(AccountID);

            if (account == null) {
                return 3;
            }

            if (account.Banned) {
                if (account.ExpiryDate > DateTime.Now) {
                    return 4;
                }
                account.Banned = false;
            }
            account.BanReason = string.Empty;
            account.ExpiryDate = DateTime.MinValue;
            if (string.CompareOrdinal(account.Password, Password) != 0) {
                if (account.WrongPasswordCount++ >= 5) {
                    account.Banned = true;
                    account.BanReason = "Too many Wrong Login Attempts.";
                    account.ExpiryDate = DateTime.Now.AddMinutes(2);
                    return 5;
                }
                return 6;
            }
            account.WrongPasswordCount = 0;
            return 7;
        }

        public void NewCharacter(ClientPackets.NewCharacter p, MirConnection c, bool IsGm) {
            if (!Settings.AllowNewCharacter) {
                c.Enqueue(new ServerPackets.NewCharacter {
                    Result = 0
                });
                return;
            }

            if (!CharacterReg.IsMatch(p.Name) || p.Name.Length < 5) {
                c.Enqueue(new ServerPackets.NewCharacter {
                    Result = 1
                });
                return;
            }

            if (!IsGm && DisabledCharNames.Contains(p.Name.ToUpper())) {
                c.Enqueue(new ServerPackets.NewCharacter {
                    Result = 1
                });
                return;
            }

            if (p.Gender != MirGender.Male && p.Gender != MirGender.Female) {
                c.Enqueue(new ServerPackets.NewCharacter {
                    Result = 2
                });
                return;
            }

            if (p.Class != MirClass.Warrior && p.Class != MirClass.Wizard && p.Class != MirClass.Taoist &&
                p.Class != MirClass.Assassin && p.Class != MirClass.Archer) {
                c.Enqueue(new ServerPackets.NewCharacter {
                    Result = 3
                });
                return;
            }

            if (p.Class == MirClass.Assassin && !Settings.AllowCreateAssassin ||
                p.Class == MirClass.Archer && !Settings.AllowCreateArcher) {
                c.Enqueue(new ServerPackets.NewCharacter {
                    Result = 3
                });
                return;
            }

            var count = 0;

            for (var i = 0; i < c.Account.Characters.Count; i++) {
                if (c.Account.Characters[i].Deleted) continue;

                if (++count >= Globals.MaxCharacterCount) {
                    c.Enqueue(new ServerPackets.NewCharacter {
                        Result = 4
                    });
                    return;
                }
            }

            lock (AccountLock) {
                if (CharacterExists(p.Name)) {
                    c.Enqueue(new ServerPackets.NewCharacter {
                        Result = 5
                    });
                    return;
                }

                var info = new CharacterInfo(p, c) {
                    Index = ++NextCharacterID,
                    AccountInfo = c.Account
                };

                c.Account.Characters.Add(info);
                CharacterList.Add(info);

                c.Enqueue(new ServerPackets.NewCharacterSuccess {
                    CharInfo = info.ToSelectInfo()
                });
            }
        }

        public bool AccountExists(string accountID) {
            for (var i = 0; i < AccountList.Count; i++)
                if (string.Compare(AccountList[i].AccountID, accountID, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;

            return false;
        }
        public bool CharacterExists(string name) {
            for (var i = 0; i < CharacterList.Count; i++)
                if (string.Compare(CharacterList[i].Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;

            return false;
        }

        private AccountInfo GetAccount(string accountID) {
            for (var i = 0; i < AccountList.Count; i++)
                if (string.Compare(AccountList[i].AccountID, accountID, StringComparison.OrdinalIgnoreCase) == 0)
                    return AccountList[i];

            return null;
        }
        public List<AccountInfo> MatchAccounts(string accountID, bool match = false) {
            if (string.IsNullOrEmpty(accountID)) return new List<AccountInfo>(AccountList);

            var list = new List<AccountInfo>();

            for (var i = 0; i < AccountList.Count; i++) {
                if (match) {
                    if (AccountList[i].AccountID.Equals(accountID, StringComparison.OrdinalIgnoreCase))
                        list.Add(AccountList[i]);
                } else {
                    if (AccountList[i].AccountID.IndexOf(accountID, StringComparison.OrdinalIgnoreCase) >= 0)
                        list.Add(AccountList[i]);
                }
            }

            return list;
        }

        public List<AccountInfo> MatchAccountsByPlayer(string playerName, bool match = false) {
            if (string.IsNullOrEmpty(playerName)) return new List<AccountInfo>(AccountList);

            var list = new List<AccountInfo>();

            for (var i = 0; i < AccountList.Count; i++) {
                for (var j = 0; j < AccountList[i].Characters.Count; j++) {
                    if (match) {
                        if (AccountList[i].Characters[j].Name.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                            list.Add(AccountList[i]);
                    } else {
                        if (AccountList[i].Characters[j].Name.IndexOf(playerName, StringComparison.OrdinalIgnoreCase) >= 0)
                            list.Add(AccountList[i]);
                    }
                }
            }

            return list;
        }

        public void CreateAccountInfo() {
            AccountList.Add(new AccountInfo {
                Index = ++NextAccountID
            });
        }
        public void CreateMapInfo() {
            MapInfoList.Add(new MapInfo {
                Index = ++MapIndex
            });
        }
        public void CreateItemInfo(ItemType type = ItemType.Nothing) {
            ItemInfoList.Add(new ItemInfo {
                Index = ++ItemIndex,
                Type = type,
                RandomStatsId = 255
            });
        }
        public void CreateMonsterInfo() {
            MonsterInfoList.Add(new MonsterInfo {
                Index = ++MonsterIndex
            });
        }
        public void CreateNPCInfo() {
            NPCInfoList.Add(new NPCInfo {
                Index = ++NPCIndex
            });
        }
        public void CreateQuestInfo() {
            QuestInfoList.Add(new QuestInfo {
                Index = ++QuestIndex
            });
        }

        public void AddToGameShop(ItemInfo Info) {
            GameShopList.Add(new GameShopItem {
                GIndex = ++GameshopIndex,
                GoldPrice = (uint)(1000 * Settings.CredxGold),
                CreditPrice = 1000,
                ItemIndex = Info.Index,
                Info = Info,
                Date = DateTime.Now,
                Class = "All",
                Category = Info.Type.ToString()
            });
        }

        public void Remove(MapInfo info) {
            MapInfoList.Remove(info);
            //Desync all objects\
        }
        public void Remove(ItemInfo info) {
            ItemInfoList.Remove(info);
        }
        public void Remove(MonsterInfo info) {
            MonsterInfoList.Remove(info);
            //Desync all objects\
        }
        public void Remove(NPCInfo info) {
            NPCInfoList.Remove(info);
            //Desync all objects\
        }
        public void Remove(QuestInfo info) {
            QuestInfoList.Remove(info);
            //Desync all objects\
        }

        public void Remove(GameShopItem info) {
            GameShopList.Remove(info);

            if (GameShopList.Count == 0) {
                GameshopIndex = 0;
            }

            //Desync all objects\
        }

        public UserItem CreateFreshItem(ItemInfo info) {
            var item = new UserItem(info) {
                UniqueID = ++NextUserItemID,
                CurrentDura = info.Durability,
                MaxDura = info.Durability
            };

            UpdateItemExpiry(item);

            return item;
        }
        public UserItem CreateDropItem(int index) {
            return CreateDropItem(GetItemInfo(index));
        }
        public UserItem CreateDropItem(ItemInfo info) {
            if (info == null) return null;

            var item = new UserItem(info) {
                UniqueID = ++NextUserItemID,
                MaxDura = info.Durability,
                CurrentDura = (ushort)Math.Min(info.Durability, Random.Next(info.Durability) + 1000)
            };

            UpgradeItem(item);

            UpdateItemExpiry(item);

            if (!info.NeedIdentify) item.Identified = true;
            return item;
        }

        public UserItem CreateShopItem(ItemInfo info) {
            if (info == null) return null;

            var item = new UserItem(info) {
                UniqueID = ++NextUserItemID,
                CurrentDura = info.Durability,
                MaxDura = info.Durability,
            };

            return item;
        }

        public void UpdateItemExpiry(UserItem item) {
            //can't have expiry on usable items
            if (item.Info.Type == ItemType.Scroll || item.Info.Type == ItemType.Potion ||
                item.Info.Type == ItemType.Transform || item.Info.Type == ItemType.Script) return;

            var expiryInfo = new ExpireInfo();

            var r = new Regex(@"\[(.*?)\]");
            var expiryMatch = r.Match(item.Info.Name);

            if (expiryMatch.Success) {
                var parameter = expiryMatch.Groups[1].Captures[0].Value;

                var numAlpha = new Regex("(?<Numeric>[0-9]*)(?<Alpha>[a-zA-Z]*)");
                var match = numAlpha.Match(parameter);

                var alpha = match.Groups["Alpha"].Value;
                var num = 0;

                int.TryParse(match.Groups["Numeric"].Value, out num);

                switch (alpha) {
                    case "m":
                        expiryInfo.ExpiryDate = DateTime.Now.AddMinutes(num);
                        break;
                    case "h":
                        expiryInfo.ExpiryDate = DateTime.Now.AddHours(num);
                        break;
                    case "d":
                        expiryInfo.ExpiryDate = DateTime.Now.AddDays(num);
                        break;
                    case "M":
                        expiryInfo.ExpiryDate = DateTime.Now.AddMonths(num);
                        break;
                    case "y":
                        expiryInfo.ExpiryDate = DateTime.Now.AddYears(num);
                        break;
                    default:
                        expiryInfo.ExpiryDate = DateTime.MaxValue;
                        break;
                }

                item.ExpireInfo = expiryInfo;
            }
        }

        public void UpgradeItem(UserItem item) {
            if (item.Info.RandomStats == null) return;
            var stat = item.Info.RandomStats;
            if (stat.MaxDuraChance > 0 && Random.Next(stat.MaxDuraChance) == 0) {
                var dura = RandomomRange(stat.MaxDuraMaxStat, stat.MaxDuraStatChance);
                item.MaxDura = (ushort)Math.Min(ushort.MaxValue, item.MaxDura + dura * 1000);
                item.CurrentDura = (ushort)Math.Min(ushort.MaxValue, item.CurrentDura + dura * 1000);
            }

            if (stat.MaxAcChance > 0 && Random.Next(stat.MaxAcChance) == 0) item.AC = (byte)(RandomomRange(stat.MaxAcMaxStat - 1, stat.MaxAcStatChance) + 1);
            if (stat.MaxMacChance > 0 && Random.Next(stat.MaxMacChance) == 0) item.MAC = (byte)(RandomomRange(stat.MaxMacMaxStat - 1, stat.MaxMacStatChance) + 1);
            if (stat.MaxDcChance > 0 && Random.Next(stat.MaxDcChance) == 0) item.DC = (byte)(RandomomRange(stat.MaxDcMaxStat - 1, stat.MaxDcStatChance) + 1);
            if (stat.MaxMcChance > 0 && Random.Next(stat.MaxScChance) == 0) item.MC = (byte)(RandomomRange(stat.MaxMcMaxStat - 1, stat.MaxMcStatChance) + 1);
            if (stat.MaxScChance > 0 && Random.Next(stat.MaxMcChance) == 0) item.SC = (byte)(RandomomRange(stat.MaxScMaxStat - 1, stat.MaxScStatChance) + 1);
            if (stat.AccuracyChance > 0 && Random.Next(stat.AccuracyChance) == 0) item.Accuracy = (byte)(RandomomRange(stat.AccuracyMaxStat - 1, stat.AccuracyStatChance) + 1);
            if (stat.AgilityChance > 0 && Random.Next(stat.AgilityChance) == 0) item.Agility = (byte)(RandomomRange(stat.AgilityMaxStat - 1, stat.AgilityStatChance) + 1);
            if (stat.HpChance > 0 && Random.Next(stat.HpChance) == 0) item.HP = (byte)(RandomomRange(stat.HpMaxStat - 1, stat.HpStatChance) + 1);
            if (stat.MpChance > 0 && Random.Next(stat.MpChance) == 0) item.MP = (byte)(RandomomRange(stat.MpMaxStat - 1, stat.MpStatChance) + 1);
            if (stat.StrongChance > 0 && Random.Next(stat.StrongChance) == 0) item.Strong = (byte)(RandomomRange(stat.StrongMaxStat - 1, stat.StrongStatChance) + 1);
            if (stat.MagicResistChance > 0 && Random.Next(stat.MagicResistChance) == 0) item.MagicResist = (byte)(RandomomRange(stat.MagicResistMaxStat - 1, stat.MagicResistStatChance) + 1);
            if (stat.PoisonResistChance > 0 && Random.Next(stat.PoisonResistChance) == 0) item.PoisonResist = (byte)(RandomomRange(stat.PoisonResistMaxStat - 1, stat.PoisonResistStatChance) + 1);
            if (stat.HpRecovChance > 0 && Random.Next(stat.HpRecovChance) == 0) item.HealthRecovery = (byte)(RandomomRange(stat.HpRecovMaxStat - 1, stat.HpRecovStatChance) + 1);
            if (stat.MpRecovChance > 0 && Random.Next(stat.MpRecovChance) == 0) item.ManaRecovery = (byte)(RandomomRange(stat.MpRecovMaxStat - 1, stat.MpRecovStatChance) + 1);
            if (stat.PoisonRecovChance > 0 && Random.Next(stat.PoisonRecovChance) == 0) item.PoisonRecovery = (byte)(RandomomRange(stat.PoisonRecovMaxStat - 1, stat.PoisonRecovStatChance) + 1);
            if (stat.CriticalRateChance > 0 && Random.Next(stat.CriticalRateChance) == 0) item.CriticalRate = (byte)(RandomomRange(stat.CriticalRateMaxStat - 1, stat.CriticalRateStatChance) + 1);
            if (stat.CriticalDamageChance > 0 && Random.Next(stat.CriticalDamageChance) == 0) item.CriticalDamage = (byte)(RandomomRange(stat.CriticalDamageMaxStat - 1, stat.CriticalDamageStatChance) + 1);
            if (stat.FreezeChance > 0 && Random.Next(stat.FreezeChance) == 0) item.Freezing = (byte)(RandomomRange(stat.FreezeMaxStat - 1, stat.FreezeStatChance) + 1);
            if (stat.PoisonAttackChance > 0 && Random.Next(stat.PoisonAttackChance) == 0) item.PoisonAttack = (byte)(RandomomRange(stat.PoisonAttackMaxStat - 1, stat.PoisonAttackStatChance) + 1);
            if (stat.AttackSpeedChance > 0 && Random.Next(stat.AttackSpeedChance) == 0) item.AttackSpeed = (sbyte)(RandomomRange(stat.AttackSpeedMaxStat - 1, stat.AttackSpeedStatChance) + 1);
            if (stat.LuckChance > 0 && Random.Next(stat.LuckChance) == 0) item.Luck = (sbyte)(RandomomRange(stat.LuckMaxStat - 1, stat.LuckStatChance) + 1);
            if (stat.CurseChance > 0 && Random.Next(100) <= stat.CurseChance) item.Cursed = true;
        }

        public int RandomomRange(int count, int rate) {
            var x = 0;
            for (var i = 0; i < count; i++)
                if (Random.Next(rate) == 0)
                    x++;
            return x;
        }
        public bool BindItem(UserItem item) {
            for (var i = 0; i < ItemInfoList.Count; i++) {
                var info = ItemInfoList[i];
                if (info.Index != item.ItemIndex) continue;
                item.Info = info;

                return BindSlotItems(item);
            }
            return false;
        }

        public bool BindGameShop(GameShopItem item, bool editEnvir = true) {
            for (var i = 0; i < Edit.ItemInfoList.Count; i++) {
                var info = Edit.ItemInfoList[i];
                if (info.Index != item.ItemIndex) continue;
                item.Info = info;

                return true;
            }
            return false;
        }

        public bool BindSlotItems(UserItem item) {
            for (var i = 0; i < item.Slots.Length; i++) {
                if (item.Slots[i] == null) continue;

                if (!BindItem(item.Slots[i])) return false;
            }

            item.SetSlotSize();

            return true;
        }

        public bool BindQuest(QuestProgressInfo quest) {
            for (var i = 0; i < QuestInfoList.Count; i++) {
                var info = QuestInfoList[i];
                if (info.Index != quest.Index) continue;
                quest.Info = info;
                return true;
            }
            return false;
        }

        public Map GetMap(int index) {
            return MapList.FirstOrDefault(t => t.Info.Index == index);
        }

        public Map GetMapByNameAndInstance(string name, int instanceValue = 0) {
            if (instanceValue < 0) instanceValue = 0;
            if (instanceValue > 0) instanceValue--;

            var instanceMapList = MapList.Where(t => string.Equals(t.Info.FileName, name, StringComparison.CurrentCultureIgnoreCase)).ToList();
            return instanceValue < instanceMapList.Count() ? instanceMapList[instanceValue] : null;
        }

        public MapObject GetObject(uint objectID) {
            return Objects.FirstOrDefault(e => e.ObjectID == objectID);
        }

        public MonsterInfo GetMonsterInfo(int index) {
            for (var i = 0; i < MonsterInfoList.Count; i++)
                if (MonsterInfoList[i].Index == index)
                    return MonsterInfoList[i];

            return null;
        }

        public NPCObject GetNPC(string name) {
            return MapList.SelectMany(t1 => t1.NPCs.Where(t => t.Info.Name == name)).FirstOrDefault();
        }

        public MonsterInfo GetMonsterInfo(string name, bool Strict = false) {
            for (var i = 0; i < MonsterInfoList.Count; i++) {
                var info = MonsterInfoList[i];
                if (Strict) {
                    if (info.Name != name) continue;
                    return info;
                } else {
                    //if (info.Name != name && !info.Name.Replace(" ", "").StartsWith(name, StringComparison.OrdinalIgnoreCase)) continue;
                    if (string.Compare(info.Name, name, StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(info.Name.Replace(" ", ""), name.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) != 0) continue;
                    return info;
                }
            }
            return null;
        }
        public PlayerObject GetPlayer(string name) {
            for (var i = 0; i < Players.Count; i++)
                if (string.Compare(Players[i].Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return Players[i];

            return null;
        }
        public PlayerObject GetPlayer(uint PlayerId) {
            for (var i = 0; i < Players.Count; i++)
                if (Players[i].Info.Index == PlayerId)
                    return Players[i];

            return null;
        }
        public CharacterInfo GetCharacterInfo(string name) {
            for (var i = 0; i < CharacterList.Count; i++)
                if (string.Compare(CharacterList[i].Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return CharacterList[i];

            return null;
        }

        public CharacterInfo GetCharacterInfo(int index) {
            for (var i = 0; i < CharacterList.Count; i++)
                if (CharacterList[i].Index == index)
                    return CharacterList[i];

            return null;
        }

        public ItemInfo GetItemInfo(int index) {
            for (var i = 0; i < ItemInfoList.Count; i++) {
                var info = ItemInfoList[i];
                if (info.Index != index) continue;
                return info;
            }
            return null;
        }
        public ItemInfo GetItemInfo(string name) {
            for (var i = 0; i < ItemInfoList.Count; i++) {
                var info = ItemInfoList[i];
                if (string.Compare(info.Name.Replace(" ", ""), name, StringComparison.OrdinalIgnoreCase) != 0) continue;
                return info;
            }
            return null;
        }
        public QuestInfo GetQuestInfo(int index) {
            return QuestInfoList.FirstOrDefault(info => info.Index == index);
        }

        public ItemInfo GetBook(short Skill) {
            for (var i = 0; i < ItemInfoList.Count; i++) {
                var info = ItemInfoList[i];
                if (info.Type != ItemType.Book || info.Shape != Skill) continue;
                return info;
            }
            return null;
        }

        public void MessageAccount(AccountInfo account, string message, ChatType type) {
            if (account?.Characters == null) return;

            for (var i = 0; i < account.Characters.Count; i++) {
                if (account.Characters[i].Player == null) continue;
                account.Characters[i].Player.ReceiveChat(message, type);
                return;
            }
        }
        public GuildObject GetGuild(string name) {
            for (var i = 0; i < GuildList.Count; i++) {
                if (string.Compare(GuildList[i].Name.Replace(" ", ""), name, StringComparison.OrdinalIgnoreCase) != 0) continue;
                return GuildList[i];
            }
            return null;
        }
        public GuildObject GetGuild(int index) {
            for (var i = 0; i < GuildList.Count; i++)
                if (GuildList[i].Guildindex == index)
                    return GuildList[i];
            return null;
        }

        public void ProcessNewDay() {
            foreach (var c in CharacterList) {
                ClearDailyQuests(c);

                c.NewDay = true;

                c.Player?.CallDefaultNPC(DefaultNPCType.Daily);
            }
        }

        private void ProcessRentedItems() {
            foreach (var characterInfo in CharacterList) {
                if (characterInfo.RentedItems.Count <= 0)
                    continue;

                foreach (var rentedItemInfo in characterInfo.RentedItems) {
                    if (rentedItemInfo.ItemReturnDate >= Now)
                        continue;

                    var rentingPlayer = GetCharacterInfo(rentedItemInfo.RentingPlayerName);

                    for (var i = 0; i < rentingPlayer.Inventory.Length; i++) {
                        if (rentedItemInfo.ItemId != rentingPlayer?.Inventory[i]?.UniqueID)
                            continue;

                        var item = rentingPlayer.Inventory[i];

                        if (item?.RentalInformation == null)
                            continue;

                        if (Now <= item.RentalInformation.ExpiryDate)
                            continue;

                        ReturnRentalItem(item, item.RentalInformation.OwnerName, rentingPlayer, false);
                        rentingPlayer.Inventory[i] = null;
                        rentingPlayer.HasRentedItem = false;

                        if (rentingPlayer.Player == null)
                            continue;

                        rentingPlayer.Player.ReceiveChat($"{item.Info.FriendlyName} has just expired from your inventory.", ChatType.Hint);
                        rentingPlayer.Player.Enqueue(new S.DeleteItem {
                            UniqueID = item.UniqueID,
                            Count = item.Count
                        });
                        rentingPlayer.Player.RefreshStats();
                    }

                    for (var i = 0; i < rentingPlayer.Equipment.Length; i++) {
                        var item = rentingPlayer.Equipment[i];

                        if (item?.RentalInformation == null)
                            continue;

                        if (Now <= item.RentalInformation.ExpiryDate)
                            continue;

                        ReturnRentalItem(item, item.RentalInformation.OwnerName, rentingPlayer, false);
                        rentingPlayer.Equipment[i] = null;
                        rentingPlayer.HasRentedItem = false;

                        if (rentingPlayer.Player == null)
                            continue;

                        rentingPlayer.Player.ReceiveChat($"{item.Info.FriendlyName} has just expired from your inventory.", ChatType.Hint);
                        rentingPlayer.Player.Enqueue(new S.DeleteItem {
                            UniqueID = item.UniqueID,
                            Count = item.Count
                        });
                        rentingPlayer.Player.RefreshStats();
                    }
                }
            }

            foreach (var characterInfo in CharacterList) {
                if (characterInfo.RentedItemsToRemove.Count <= 0)
                    continue;

                foreach (var rentalInformationToRemove in characterInfo.RentedItemsToRemove)
                    characterInfo.RentedItems.Remove(rentalInformationToRemove);

                characterInfo.RentedItemsToRemove.Clear();
            }
        }

        public bool ReturnRentalItem(UserItem rentedItem, string ownerName, CharacterInfo rentingCharacterInfo, bool removeNow = true) {
            if (rentedItem.RentalInformation == null)
                return false;

            var owner = GetCharacterInfo(ownerName);
            var returnItems = new List<UserItem>();

            foreach (var rentalInformation in owner.RentedItems)
                if (rentalInformation.ItemId == rentedItem.UniqueID)
                    owner.RentedItemsToRemove.Add(rentalInformation);

            rentedItem.RentalInformation.BindingFlags = BindMode.none;
            rentedItem.RentalInformation.RentalLocked = true;
            rentedItem.RentalInformation.ExpiryDate = rentedItem.RentalInformation.ExpiryDate.AddDays(1);

            returnItems.Add(rentedItem);

            var mail = new MailInfo(owner.Index, true) {
                Sender = rentingCharacterInfo.Name,
                Message = rentedItem.Info.FriendlyName,
                Items = returnItems
            };

            mail.Send();

            if (removeNow) {
                foreach (var rentalInformationToRemove in owner.RentedItemsToRemove)
                    owner.RentedItems.Remove(rentalInformationToRemove);

                owner.RentedItemsToRemove.Clear();
            }

            return true;
        }

        private void ClearDailyQuests(CharacterInfo info) {
            foreach (var quest in QuestInfoList) {
                if (quest.Type != QuestType.Daily) continue;

                for (var i = 0; i < info.CompletedQuests.Count; i++) {
                    if (info.CompletedQuests[i] != quest.Index) continue;

                    info.CompletedQuests.RemoveAt(i);
                }
            }

            info.Player?.GetCompletedQuests();
        }

        public GuildBuffInfo FindGuildBuffInfo(int Id) {
            for (var i = 0; i < Settings.Guild_BuffList.Count; i++)
                if (Settings.Guild_BuffList[i].Id == Id)
                    return Settings.Guild_BuffList[i];
            return null;
        }

        public void ClearGameshopLog() {
            Main.GameshopLog.Clear();

            for (var i = 0; i < AccountList.Count; i++) {
                for (var f = 0; f < AccountList[i].Characters.Count; f++) {
                    AccountList[i].Characters[f].GSpurchases.Clear();
                }
            }

            ResetGS = false;
            MessageQueue.Enqueue("Gameshop Purchase Logs Cleared.");

        }

        int RankCount = 100; //could make this a global but it made sence since this is only used here, it should stay here

        public int InsertRank(List<Rank_Character_Info> Ranking, Rank_Character_Info NewRank) {
            if (Ranking.Count == 0) {
                Ranking.Add(NewRank);
                return Ranking.Count;
            }

            for (var i = 0; i < Ranking.Count; i++) {
                //if level is lower
                if (Ranking[i].level < NewRank.level) {
                    Ranking.Insert(i, NewRank);
                    return i + 1;
                }

                //if exp is lower but level = same
                if (Ranking[i].level == NewRank.level && Ranking[i].Experience < NewRank.Experience) {
                    Ranking.Insert(i, NewRank);
                    return i + 1;
                }
            }

            if (Ranking.Count < RankCount) {
                Ranking.Add(NewRank);
                return Ranking.Count;
            }

            return 0;
        }

        public bool TryAddRank(List<Rank_Character_Info> Ranking, CharacterInfo info, byte type) {
            var NewRank = new Rank_Character_Info() {
                Name = info.Name,
                Class = info.Class,
                Experience = info.Experience,
                level = info.Level,
                PlayerId = info.Index,
                info = info
            };
            var NewRankIndex = InsertRank(Ranking, NewRank);
            if (NewRankIndex == 0) return false;
            for (var i = NewRankIndex; i < Ranking.Count; i++) {
                SetNewRank(Ranking[i], i + 1, type);
            }
            info.Rank[type] = NewRankIndex;
            return true;
        }

        public int FindRank(List<Rank_Character_Info> Ranking, CharacterInfo info, byte type) {
            var startindex = info.Rank[type];
            if (startindex > 0) //if there's a previously known rank then the user can only have gone down in the ranking (or stayed the same)
            {
                for (var i = startindex - 1; i < Ranking.Count; i++) {
                    if (Ranking[i].Name == info.Name)
                        return i;
                }
                info.Rank[type] = 0; //set the rank to 0 to tell future searches it's not there anymore
            }
            return -1; //index can be 0
        }

        public bool UpdateRank(List<Rank_Character_Info> Ranking, CharacterInfo info, byte type) {
            var CurrentRank = FindRank(Ranking, info, type);
            if (CurrentRank == -1) return false; //not in ranking list atm

            var NewRank = CurrentRank;
            //next find our updated rank
            for (var i = CurrentRank - 1; i >= 0; i--) {
                if (Ranking[i].level > info.Level || Ranking[i].level == info.Level && Ranking[i].Experience > info.Experience) break;
                NewRank = i;
            }

            Ranking[CurrentRank].level = info.Level;
            Ranking[CurrentRank].Experience = info.Experience;

            if (NewRank < CurrentRank) { //if we gained any ranks
                Ranking.Insert(NewRank, Ranking[CurrentRank]);
                Ranking.RemoveAt(CurrentRank + 1);
                for (var i = NewRank + 1; i < Math.Min(Ranking.Count, CurrentRank + 1); i++) {
                    SetNewRank(Ranking[i], i + 1, type);
                }
            }
            info.Rank[type] = NewRank + 1;

            return true;
        }

        public void SetNewRank(Rank_Character_Info Rank, int Index, byte type) {
            if (!(Rank.info is CharacterInfo Player)) return;
            Player.Rank[type] = Index;
        }

        public void RemoveRank(CharacterInfo info) {
            List<Rank_Character_Info> Ranking;
            var Rankindex = -1;
            //first check overall top           
            if (info.Level >= RankBottomLevel[0]) {
                Ranking = RankTop;
                Rankindex = FindRank(Ranking, info, 0);
                if (Rankindex >= 0) {
                    Ranking.RemoveAt(Rankindex);
                    for (var i = Rankindex; i < Ranking.Count(); i++) {
                        SetNewRank(Ranking[i], i, 0);
                    }
                }
            }
            //next class based top
            if (info.Level < RankBottomLevel[(byte)info.Class + 1]) return;
            {
                Ranking = RankTop;
                Rankindex = FindRank(Ranking, info, 1);
                if (Rankindex >= 0) {
                    Ranking.RemoveAt(Rankindex);
                    for (var i = Rankindex; i < Ranking.Count(); i++) {
                        SetNewRank(Ranking[i], i, 1);
                    }
                }
            }
        }

        public void CheckRankUpdate(CharacterInfo info) {
            List<Rank_Character_Info> Ranking;
            Rank_Character_Info NewRank;

            //first check overall top           
            if (info.Level >= RankBottomLevel[0]) {
                Ranking = RankTop;
                if (!UpdateRank(Ranking, info, 0)) {
                    if (TryAddRank(Ranking, info, 0)) {
                        if (Ranking.Count > RankCount) {
                            SetNewRank(Ranking[RankCount], 0, 0);
                            Ranking.RemoveAt(RankCount);

                        }
                    }
                }
                if (Ranking.Count >= RankCount) {
                    NewRank = Ranking[Ranking.Count - 1];
                    if (NewRank != null)
                        RankBottomLevel[0] = NewRank.level;
                }
            }
            //now check class top
            if (info.Level >= RankBottomLevel[(byte)info.Class + 1]) {
                Ranking = _rankClass[(byte)info.Class];
                if (!UpdateRank(Ranking, info, 1)) {
                    if (TryAddRank(Ranking, info, 1)) {
                        if (Ranking.Count > RankCount) {
                            SetNewRank(Ranking[RankCount], 0, 1);
                            Ranking.RemoveAt(RankCount);
                        }
                    }
                }

                if (Ranking.Count < RankCount) return;
                NewRank = Ranking[Ranking.Count - 1];
                if (NewRank != null)
                    RankBottomLevel[(byte)info.Class + 1] = NewRank.level;
            }
        }


        public void ReloadNPCs() {
            var allNpcs = new List<NPCObject>();
            foreach (var map in MapList) {
                allNpcs.AddRange(map.NPCs);
            }
            foreach (var item in allNpcs) {
                item.LoadInfo(true);
            }
            Main.DefaultNPC.LoadInfo(true);
            MessageQueue.Enqueue("NPCs reloaded...");
        }

        public void ReloadDrops() {
            foreach (var item in MonsterInfoList)
                item.LoadDrops();
            MessageQueue.Enqueue("Drops reloaded...");
        }

    }
}
