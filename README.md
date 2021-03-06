**Speed Type** Quickly type randomly generated words to win a prize.

## Features

- Randomly generated words with configurable lengths.
- Multiple reward tiers.
- Configurable rewards per tier.
- Automated events.

## Planed Features

- Leaderboards system with either a 3/5/10 list of the top speed typers!
  - With the setting to clear the leaderboard on wipe.
- Ability to choose between randomly generated strings and predetermined words/sentences

## Commands

- **/guess <word>** 	-- Type the random generated word to win a prize.
- **/guess start t1** 	-- Manually Start Tier 1 Event ***(requires `speedtype.admin` permission)***
- **/guess start t2** 	-- Manually Start Tier 2 Event ***(requires `speedtype.admin` permission)***
- **/guess start t3** 	-- Manually Start Tier 3 Event ***(requires `speedtype.admin` permission)***
- **/guess end** 		-- Ends any ongoing events ***(requires `speedtype.admin` permission)***

## Permissions

- **speedtype.admin** -- Required to be able to use the console command to start an event

## Configuration

```json
{
  "Enable Automatic Events": true,
  "Event Frequency (Run Event Every X Seconds)": 300.0,
  "Event Length (Ends After X Seconds)": 60.0,
  "Minimum number of players to start a event": 10,
  "Chat Icon (SteamID64)": 0,
  "Tier 1 Letters/Numbers count": 6,
  "Enable Tier 2 Events": false,
  "Tier 2 Event Frequency (Every X events it will be a tier 2 event)": 10,
  "Tier 2 Letters/Numbers count": 10,
  "Enable Tier 3 Events": false,
  "Tier 3 Event Frequency (Every X events it will be a tier 3 event)": 100,
  "Tier 3 Letters/Numbers count": 14,
  "Tier 1 Loot (Item Shortname | Item Ammount)": [
    {
      "stones": 100,
      "wood": 100
    },
    {
      "bandage": 100
    }
  ],
  "Tier 2 Loot (Item Shortname | Item Ammount)": [
    {
      "metal.fragments": 50,
      "metal.refined": 20
    },
    {
      "leather": 60,
      "cloth": 40
    }
  ],
  "Tier 3 Loot (Item Shortname | Item Ammount)": [
    {
      "explosive.timed": 1
    },
    {
      "rifle.ak": 1,
      "ammo.rifle": 50
    }
  ],
  "Log Events to console": false
}
```

## Localization

The default messages are in the `SpeedType.json` file under the `oxide/lang/en` directory.

```json
{
  "EventStart": "<size=20><color=#1e90ff>Speed Type</color></size>\n<size=16><color=#{0}>Tier {1} Event</color></size>\n\nThe first person to type:\n<color=#33ccff>/guess {2}</color>\nWill win a prize!",
  "EventEnd": "<size=20><color=#1e90ff>Speed Type</color></size>\n<size=16><color=#ffa500>Event Over!</color></size>\n\nNo Winners",
  "EventEndWinner": "<size=20><color=#1e90ff>Speed Type</color></size>\n<size=16><color=#ffa500>Event Over!</color></size>\n\nThe Winner is:\n<color=#1e90ff>{0}</color>\nReward:{1}",
  "EventNotStarted": "<size=20><color=#1e90ff>Speed Type</color></size>\n\n<size=16><color=#ffa500>No Active Events!</color></size>",
  "EventStarted": "<size=20><color=#1e90ff>Speed Type</color></size>\n\n<size=16><color=#ffa500>Event already started</color></size>",
  "LogEventStart": "Speed Type Tier {0} Event Started",
  "LogEventEnd": "Speed Type Event Ended",
  "LogEventEndWinner": "Speed Type Event Winner: {0} | User Won: {1} x{2}",
  "WrongCode": "<size=20><color=#1e90ff>Speed Type</color></size>\n\n<size=16><color=#ffa500>Wrong Code!</color></size>",
  "WrongSyntax": "<size=20><color=#1e90ff>Speed Type</color></size>\n\n<size=16><color=#ffa500>Wrong Command Syntax</color></size>",
  "WrongPerm": "<size=20><color=#1e90ff>Speed Type</color></size>\n\n<size=16><color=#ffa500>No Permission!</color></size>"
}
```

## Credits

- **Boris Yurinov** -- Logo idea and creation!