using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Oxide.Core.Configuration;
using Oxide.Core;
using Random = UnityEngine.Random;

namespace Oxide.Plugins
{
	[Info("Speed Guess", "TMafono", "1.1.0")]
    [Description("Quickly guess random generated words to win a prize")]
    class SpeedGuess : RustPlugin
    {
		#region Variables
		private const string SpeedGuessAdmin = "speedguess.admin";
		
		private bool EventActive = false;
		private string RandomWord = "";
		
		private bool StartTier2Event = false;
		private bool StartTier3Event = false;
		
		Timer EndEventTimer;
        Timer EventAutoTimer;
		
		private List<string> EventWords = new List<string> {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z","0","1","2","3","4","5","6","7","8","9"};
		
		private readonly DynamicConfigFile dataFile = Interface.Oxide.DataFileSystem.GetFile("SpeedGuess");

        private Dictionary<string, int> TierStates = new Dictionary<string, int>();
		#endregion Variables
		
		#region Configuration
		private static Configuration config;

		private class Configuration
        {
			[JsonProperty(PropertyName = "Enable Automatic Events")]
            public bool AutoEventEnabled = true;
			
			[JsonProperty(PropertyName = "Event Frequency (Run Event Every X Seconds)")]
            public float EventFrequency = 300f;
			
			[JsonProperty(PropertyName = "Event Length (Ends After X Seconds)")]
            public float EventLength = 60f;
			
			[JsonProperty(PropertyName = "Chat Icon (SteamID64)")]
            public ulong ChatIcon = 0;
			
			[JsonProperty(PropertyName = "Tier 1 Letters/Numbers count")]
            public int Tier1LetterCount = 6;
			
			[JsonProperty(PropertyName = "Enable Tier 2 Events")]
            public bool Tier2EventStatus = false;
			
			[JsonProperty(PropertyName = "Tier 2 Event Frequency (Every X events it will be a tier 2 event)")]
            public int Tier2Frequency = 10;
			
			[JsonProperty(PropertyName = "Tier 2 Letters/Numbers count")]
            public int Tier2LetterCount = 10;
			
			[JsonProperty(PropertyName = "Enable Tier 3 Events")]
            public bool Tier3EventStatus = false;
			
			[JsonProperty(PropertyName = "Tier 3 Event Frequency (Every X events it will be a tier 3 event)")]
            public int Tier3Frequency = 100;
			
			[JsonProperty(PropertyName = "Tier 3 Letters/Numbers count")]
            public int Tier3LetterCount = 14;
			
			[JsonProperty(PropertyName = "Tier 1 Loot (Item Shortname | Item Ammount)", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, int> EventT1LootTable = new Dictionary<string, int>
            {
                {"stones", 100},
                {"wood", 100}
            };
			
			[JsonProperty(PropertyName = "Tier 2 Loot (Item Shortname | Item Ammount)", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, int> EventT2LootTable = new Dictionary<string, int>
            {
                {"metal.fragments", 50},
                {"metal.refined", 20}
            };
			
			[JsonProperty(PropertyName = "Tier 3 Loot (Item Shortname | Item Ammount)", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, int> EventT3LootTable = new Dictionary<string, int>
            {
                {"explosive.timed", 1},
                {"rifle.ak", 1}
            };
			
			[JsonProperty(PropertyName = "Log Events to console")]
            public bool LogEvents = false;
        }
		
		protected override void LoadConfig()
        {
            base.LoadConfig();
            try {
                config = Config.ReadObject<Configuration>();
                if (config == null) throw new Exception();
            } catch {
                PrintError("Could not load a valid configuration file. Using default configuration values.");
                LoadDefaultConfig();
            }
        }

        protected override void LoadDefaultConfig() => config = new Configuration();

        protected override void SaveConfig() => Config.WriteObject(config);
		#endregion Configuration
		
		#region Localization
        private new void LoadDefaultMessages()
        {
            // English
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["EventStart"] = "<size=22><color=#1e90ff>Speed Guess</color></size>\n<size=17><color=#{0}>Tier {1} Event</color></size>\n\nThe first person to type:\n<color=#33ccff>/guess {2}</color>\nWill win a prize!",
				["EventEnd"] = "<size=22><color=#1e90ff>Speed Guess</color></size>\n<size=17><color=#ffa500>Event Over!</color></size>\n\nNo Winners",
				["EventEndWinner"] = "<size=22><color=#1e90ff>Speed Guess</color></size>\n<size=17><color=#ffa500>Event Over!</color></size>\n\nThe Winner is:\n<color=#1e90ff>{0}</color>",
				["EventNotStarted"] = "<size=22><color=#1e90ff>Speed Guess</color></size>\n\n<size=17><color=#ffa500>No Active Events!</color></size>",
				["EventStarted"] = "<size=22><color=#1e90ff>Speed Guess</color></size>\n\n<size=17><color=#ffa500>Event already started</color></size>",
				["LogEventStart"] = "Speed Guess Tier {0} Event Started",
				["LogEventEnd"] = "Speed Guess Event Ended",
				["LogEventEndWinner"] = "Speed Guess Event Winner: {0} | User Won: {1} x{2}",
				["WrongCode"] = "<size=22><color=#1e90ff>Speed Guess</color></size>\n\n<size=17><color=#ffa500>Wrong Code!</color></size>",
				["WrongSyntax"] = "<size=22><color=#1e90ff>Speed Guess</color></size>\n\n<size=17><color=#ffa500>Wrong Command Syntax</color></size>",
				["WrongPerm"] = "<size=22><color=#1e90ff>Speed Guess</color></size>\n\n<size=17><color=#ffa500>No Permission!</color></size>",
            }, this);
        }
        #endregion Localization
		
		#region Initialization
		private void Init()
        {
            permission.RegisterPermission(SpeedGuessAdmin, this);
			
			if(config.Tier2EventStatus || config.Tier3EventStatus) {
				TierStates = dataFile.ReadObject<Dictionary<string, int>>();
			
				if(TierStates.Count == 0) {
					TierStates = new Dictionary<string, int>
					{
						{"t2efrequency", config.Tier2Frequency},
						{"t3efrequency", config.Tier3Frequency}
					};
					dataFile.WriteObject(TierStates);
				}
			}
		}
		#endregion Initialization
		
		#region Hooks
		private void OnServerInitialized()
        {
			if (config.AutoEventEnabled) {
				EventAutoTimer = timer.Repeat(config.EventFrequency, 0, () =>
                {
                    StartSpeedGuessEvent();
                });
			}
		}
		#endregion Hooks
		
		void StartSpeedGuessEvent(bool consolecmd = false)
        {
            if (EventActive)
				return;
			
			if(!consolecmd)
				CheckTierStatus();
			
			EventActive = true;
			
			if(StartTier3Event) {
				RandomWord = SpeedEventWordGenerator(config.Tier3LetterCount);
				Broadcast(Lang("EventStart",null,"ff4500","3",RandomWord));
				if(config.LogEvents)
					Puts(Lang("LogEventStart",null,"3"));
			} else if (StartTier2Event) {
				RandomWord = SpeedEventWordGenerator(config.Tier2LetterCount);
				Broadcast(Lang("EventStart",null,"ffa500","2",RandomWord));
				if(config.LogEvents)
					Puts(Lang("LogEventStart",null,"2"));
			} else {
				RandomWord = SpeedEventWordGenerator(config.Tier1LetterCount);
				Broadcast(Lang("EventStart",null,"ffff00","1",RandomWord));
				if(config.LogEvents)
					Puts(Lang("LogEventStart",null,"1"));
			}
            
            EndEventTimer = timer.Once(config.EventLength, () =>
            {
				EndSpeedGuessEvent();
            });
        }
		
		private void EndSpeedGuessEvent(BasePlayer winner = null)
        {	
			EventActive = false;
			EndEventTimer.Destroy();
			
			if(winner != null){
				Broadcast(Lang("EventEndWinner",null,winner.displayName));

				if(StartTier3Event) {
					var randomitem = Convert.ToInt32(Math.Round(Convert.ToDouble(Random.Range(Convert.ToSingle(0), Convert.ToSingle(config.EventT3LootTable.Count-1)))));
					GiveItem(winner,config.EventT3LootTable.Keys.ElementAt(randomitem),config.EventT3LootTable.Values.ElementAt(randomitem));
				} else if (StartTier2Event) {
					var randomitem = Convert.ToInt32(Math.Round(Convert.ToDouble(Random.Range(Convert.ToSingle(0), Convert.ToSingle(config.EventT2LootTable.Count-1)))));
					GiveItem(winner,config.EventT2LootTable.Keys.ElementAt(randomitem),config.EventT2LootTable.Values.ElementAt(randomitem));
				} else {
					var randomitem = Convert.ToInt32(Math.Round(Convert.ToDouble(Random.Range(Convert.ToSingle(0), Convert.ToSingle(config.EventT1LootTable.Count-1)))));
					GiveItem(winner,config.EventT1LootTable.Keys.ElementAt(randomitem),config.EventT1LootTable.Values.ElementAt(randomitem));
				}
				if(config.LogEvents)
					Puts(Lang("LogEventEnd",null));
			} else {
				Broadcast(Lang("EventEnd",null));
				if(config.LogEvents)
					Puts(Lang("LogEventEnd",null));
			}
			
			StartTier2Event = false;
			StartTier3Event = false;
        }
		
		[ChatCommand("guess")]
        private void SpeedGuessCommand(BasePlayer player, string cmd, string[] args)
        {
			if (args.Length == 1) {
				if (!EventActive) {
					Message(player,Lang("EventNotStarted",null));
					return;
				}
				
				if(args[0] == RandomWord) {
					EndSpeedGuessEvent(player);
				} else {
					Message(player,Lang("WrongCode",null));
				}
			} else if (args.Length == 2) {
				if(args[0] == "start") {
					if(HasPermission(player)) {
						if(args[1] == "t1") {
							if (EventActive){
								Message(player,Lang("EventStarted",null));
								return;
							}
							
							StartSpeedGuessEvent(true);
						} else if(args[1] == "t2") {
							if (EventActive){
								Message(player,Lang("EventStarted",null));
								return;
							}
							
							StartTier2Event = true;
							StartSpeedGuessEvent(true);
						} else if(args[1] == "t3") {
							if (EventActive){
								Message(player,Lang("EventStarted",null));
								return;
							}
							
							StartTier3Event = true;
							StartSpeedGuessEvent(true);
						} else {
							Message(player,Lang("WrongSyntax",null));
						}
					} else {
						Message(player,Lang("WrongPerm",null));
					}
				}
			} else {
				Message(player,Lang("WrongSyntax",null));
			}
		}
		
		#region Helpers
		private void GiveItem(BasePlayer player, string itemName, int itemAmount = 1)
        {
			Item item = ItemManager.Create(FindItem(itemName));
            if (item == null) {
                return;
            }

            item.amount = itemAmount;

            ItemContainer itemContainer = player.inventory.containerMain;

            if (!player.inventory.GiveItem(item, itemContainer)) {
                item.Remove();
                return;
            }

            itemName = item.info.displayName.english;
			player.Command("note.inv", item.info.itemid, itemAmount);
			if(config.LogEvents)
				Puts(Lang("LogEventEndWinner",null,player.displayName,itemName,itemAmount));
		}
		
		private void CheckTierStatus()
        {
			if(TierStates.Count != 0) {
				if(config.Tier2EventStatus) {
					if(TierStates["t2efrequency"] == 0){
						StartTier2Event = true;
						TierStates["t2efrequency"] = config.Tier2Frequency;
					} else {
						TierStates["t2efrequency"]--;
					}
				}
				
				if(config.Tier3EventStatus) {
					if(TierStates["t3efrequency"] == 0){
						StartTier2Event = false;
						StartTier3Event = true;
						TierStates["t3efrequency"] = config.Tier3Frequency;
					} else {
						TierStates["t3efrequency"]--;
					}
				}
				
				if(config.Tier2EventStatus || config.Tier3EventStatus) {
					dataFile.WriteObject(TierStates);
				}
			}
		}
		
		private ItemDefinition FindItem(string itemName)
        {
            ItemDefinition itemDef = ItemManager.FindItemDefinition(itemName.ToLower());
            return itemDef;
        }
		
		private string SpeedEventWordGenerator(int wordcount)
		{
			var RandomGeneratedWord = "";
			
			for (var i = 0; i < wordcount; i++) {
				var randomletter = Convert.ToInt32(Math.Round(Convert.ToDouble(Random.Range(Convert.ToSingle(0), Convert.ToSingle(EventWords.Count-1)))));
				RandomGeneratedWord = RandomGeneratedWord + EventWords[randomletter];
            }
			
			return RandomGeneratedWord;
			
		}
        private void Broadcast(string message)
        {
            Server.Broadcast(message, config.ChatIcon);
        }

        private void Message(BasePlayer player, string message)
        {
            Player.Message(player, message, config.ChatIcon);
        }

        private bool HasPermission(BasePlayer player)
        {
            return permission.UserHasPermission(player.UserIDString, SpeedGuessAdmin);
        }

        private string Lang(string key, string id = null, params object[] args)
        {
            return string.Format(lang.GetMessage(key, this, id), args);
        }
		#endregion Helpers
	}
}