# Field Descriptions

This document describes all possible fields that can be used for Custom Monsters dictionary entries at `aedenthorn.CustomMonsters/dict`. For an example of an actual Content Patcher content.json file, [click here](https://github.com/aedenthorn/StardewValleyMods/blob/master/CustomMonsters/doc/content.json).


## General

### Type
The vanilla monster type to base the custom monster type on. Possible values:

	AngryRoger
	Bat
	BigSlime
	BlueSquid
	Bug
	DinoMonster
	Duggy
	DustSpirit
	DwarvishSentry
	Fly
	Ghost
	GreenSlime
	Grub
	HotHead
	LavaLurk
	Leaper
	MetalHead
	Mummy
	RockCrab
	RockGolem
	Serpent
	ShadowBrute
	ShadowGirl
	ShadowGuy
	ShadowShaman
	Shooter
	Skeleton
	Spiker
	SquidKid

### Parameters
comma-separated list of the parameters that describe the c# constructor used to instantiate the custom monster type (default "position"). Possible parameter types:

	position - refers to the position passed to the API by your spawning mod (e.g. Dynamic Map Tiles)
	level - taken from the Level field
	facing - taken from the Facing field
	name - taken from the Name field
	color - taken from the Color field
	switch - taken from the Switch field

Some of these will work differently for different monster types. See each corresponding field for more info.

[A list of constructors is here.](https://github.com/aedenthorn/StardewValleyMods/blob/master/CustomMonsters/doc/ctor.txt)


### Level
The level of the monster, often corresponding to a mine level. Will be passed to constructors with "level" parameter. E.g. MetalHead: "position,level".
### Facing
The facing direction (0 = up, 1 = right, 2 = down, 3 = left). For Spikers it's the direction they will sense players and move to hit them. Will be passed to constructors with "facing" parameter.
### Name
The custom subtype used for certain monsters, e.g. Serpent has "Royal Serpent". Will be passed to constructors with "name" parameter.
### Color
The color used for certain monsters, e.g. slimes. Will be passed to constructors with "color" parameter.
### Switch
A type dependent true or false value. Does very different things for different types. See the constructor data for more info. Will be passed to constructors with "switch" parameter.

### Sprite
a path to a custom or vanilla sprite. Custom paths must be implemented using Content Patcher.
### Scale
scale multiplier (causes problems for non-flying monsters if > 1).
### Damage
damage to farmer.
### HardMode
used to change the nature of some monster types, mimicking their changes for hard mode dungeons.
### Health
monster max health.
### Speed
monster speed.
### Experience
experience gained for killing the monster.
### Resilience
monster resilience.

### Slipperiness
monster slipperiness.
### LightType
an index for the vanilla light type used by certain monsters (not really implemented yet).

### HideShadow
whether to hide the monster's shadow.
### ShakeTimer
how long to shake a monster (bats).
### Drops
a list of items to replace the base type's item drops. Each item takes the following fields:

	ItemId - the qualified item id to drop.
	MinQuantity - minimum amount to drop.
	MaxQuantity - maximum amount to drop.
	Quality - dropped item quality.
	Chance - percent chance to drop (default 100).

e.g.:

		  "Drops":[
			  {
				  "ItemId":"(O)74",
				  "MinQuantity": 1,
				  "MaxQuantity":4,
				  "Chance":10
			  }
		  ]
		  
This causes the monster to have a 10% chance to drop 1-4 prismatic shards.


### MoveSound
sound when certain monsters move.
### MoveSound2
secondary move sound for a few monsters (e.g. when Spikers hit something).
### SpawnSound
sound to play when the monster spawns.
### DamageSound
sound when the monster gets hit.
### DamageSound2
secondary sound on hit in combination with the first for a few monsters.
### ArmorSound
sound made when hitting the armor of some monster types (e.g. bugs)
### DeathSound
sound to play when the monster dies.
### DeathSound2
secondary sound on death in combination with the first for a few monsters.

### ProjectileSound
sound when firing a projectile.
### ProjectileSound2
secondary projectile sound for some monsters (e.g. on bounce).
### ProjectileIndex
the vanilla index for certain monster projectiles (see the game's TileSheets/Projectiles file).
### ProjectileDebuff
custom debuff for existing debuff projectiles.
### ProjectileSprite
a custom sprite path for a monster's projectile.
### ProjectileSource
a custom source rectangle for a projectile sprite.
### ProjectileScale
custom scale for certain projectiles (e.g. DinoMonster).
### ProjectileDamage
damage value for some projectiles.
### ProjectileRange
range of some projectiles.

### MineSpawns
List of entries for causing this monster type to spawn on specific mine floors. Each entry takes the following fields:

	MinLevel - the minimum mine level for the monster to spawn.
	MaxLevel - the maximum mine level for the monster to spawn.
	Chance - percent chance for the monster to spawn instead of a given vanilla monster.
	Types - optional list of vanilla monster types to replace with this custom monster type. Possible values:
		AngryRoger,Bat,BigSlime,BlueSquid,Bug,DinoMonster,Duggy,DustSpirit,DwarvishSentry,Fly,Ghost,GreenSlime,Grub,HotHead,LavaLurk,Leaper,MetalHead,Mummy,RockCrab,RockGolem,Serpent,ShadowBrute,ShadowGirl,ShadowGuy,ShadowShaman,Shooter,Skeleton,SquidKid

### VolcanoSpawns
List of entries for causing this monster type to spawn on specific mine floors. Each entry takes the following fields:

	MinLevel - the minimum volcano level for the monster to spawn.
	MaxLevel - the maximum volcano level for the monster to spawn.
	Chance - percent chance for the monster to spawn instead of a given vanilla monster.
	Types - optional list of vanilla monster types to replace with this custom monster type. Possible values:
		AngryRoger,Bat,BigSlime,BlueSquid,Bug,DinoMonster,Duggy,DustSpirit,DwarvishSentry,Fly,Ghost,GreenSlime,Grub,HotHead,LavaLurk,Leaper,MetalHead,Mummy,RockCrab,RockGolem,Serpent,ShadowBrute,ShadowGirl,ShadowGuy,ShadowShaman,Shooter,Skeleton,SquidKid


## AngryRoger specific

### SprinkleColor
color of sprinkles.


## Bat specific

### CanLunge
lunges
### CursedDoll
is a cursed doll.
### HauntedSkull
is a haunted skull (requires "CursedDoll": true to work).
### SpeedMin
min value for max speed.
### SpeedMax
max value for max speed.
### ExtraVelocity
extra speed.


## BigSlime specific

### HeldItem
the item to display inside the monster.


## Bug specific

### Armored
whether the bug is armored.


## Mummy specific

### CrumbleSound
sound when the monster crumbles.
### UncrumbleSound
sound when the monster gets back up.
### ReviveTimer
time in ms to revive. Vanilla is 10000.


## RockCrab specific

### WaiterChance
chance to wait.
### ShellSound
sound of hitting the shell with a pickaxe.
### BreakSound
sound to make when the shell breaks.
### StickBug
true or false, is a stick bug.


## Serpent specific

### MinSegment
minimum number of segments for a royal serpent.
### MaxSegment
maximum number of segments for a royal serpent.


## Shooter specific

### ProjectileSpeed
speed of projectiles.
### ShotsPerFire
shots per fire.
### AimTime
time to aim when a player moves.
### BurstTime
time between shots.
### AimEndTime
time to aim after firing finishes.
### DesiredDistance
wants to be this far from players.
### FireRange
max firing range.

## Spiker specific

### Vulnerable
whether monster can be damaged.


## SquidKid specific

### ProjectileCount
how many projectiles to fire in one period.
### ProjectileTimer
how long in ms to wait between fire periods.






