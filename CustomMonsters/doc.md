
[b]Type[/b] - The vanilla monster type to base the custom monster type on. Possible values:

	[b]AngryRoger[/b]
	[b]Bat[/b]
	[b]BigSlime[/b]
	[b]BlueSquid[/b]
	[b]Bug[/b]
	[b]DinoMonster[/b]
	[b]Duggy[/b]
	[b]DustSpirit[/b]
	[b]DwarvishSentry[/b]
	[b]Fly[/b]
	[b]Ghost[/b]
	[b]GreenSlime[/b]
	[b]Grub[/b]
	[b]HotHead[/b]
	[b]LavaLurk[/b]
	[b]Leaper[/b]
	[b]MetalHead[/b]
	[b]Mummy[/b]
	[b]RockCrab[/b]
	[b]RockGolem[/b]
	[b]Serpent[/b]
	[b]ShadowBrute[/b]
	[b]ShadowGirl[/b]
	[b]ShadowGuy[/b]
	[b]ShadowShaman[/b]
	[b]Shooter[/b]
	[b]Skeleton[/b]
	[b]Spiker[/b]
	[b]SquidKid[/b]

[b]Parameters[/b] - comma-separated list of the parameters that describe the c# constructor used to instantiate the custom monster type (default "position"). Possible parameter types:

	[b]position[/b] - refers to the position passed to the API by your spawning mod (e.g. Dynamic Map Tiles)
	[b]level[/b] - taken from the Level field
	[b]facing[/b] - taken from the Facing field
	[b]name[/b] - taken from the Name field
	[b]color[/b] - taken from the Color field
	[b]switch[/b] - taken from the Switch field

Some of these will work differently for different monster types. See each corresponding field for more info.

A list of constructors is here.

[b]Level[/b] - The level of the monster, often corresponding to a mine level. Will be passed to constructors with "level" parameter. E.g. MetalHead: "position,level".
[b]Facing[/b] - The facing direction (0 = up, 1 = right, 2 = down, 3 = left). For Spikers it's the direction they will sense players and move to hit them. Will be passed to constructors with "facing" parameter.
[b]Name[/b] - The custom subtype used for certain monsters, e.g. Serpent has "Royal Serpent". Will be passed to constructors with "name" parameter.
[b]Color[/b] - The color used for certain monsters, e.g. slimes. Will be passed to constructors with "color" parameter.
[b]Switch[/b] - A type dependent true or false value. Does very different things for different types. See the constructor data for more info. Will be passed to constructors with "switch" parameter.

[b]Sprite[/b] - a path to a custom or vanilla sprite. Custom paths must be implemented using Content Patcher.
[b]Scale[/b] - scale multiplier (causes problems for non-flying monsters if > 1).
[b]Damage[/b] - damage to farmer.
[b]HardMode[/b] - used to change the nature of some monster types, mimicking their changes for hard mode dungeons.
[b]Health[/b] monster max health.
[b]Speed[/b] monster speed.
[b]Experience[/b] experience gained for killing the monster.
[b]Resilience[/b] monster resilience.

[b]Slipperiness[/b] monster slipperiness.
[b]LightType[/b] an index for the vanilla light type used by certain monsters (not really implemented yet).

[b]HideShadow[/b] whether to hide the monster's shadow.
[b]ShakeTimer[/b] how long to shake a monster (bats).
[b]Drops[/b] - a list of items to replace the base type's item drops. Each item takes the following fields:

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


[b]MoveSound[/b] - sound when certain monsters move.
[b]MoveSound2[/b] - secondary move sound for a few monsters (e.g. when Spikers hit something).
[b]SpawnSound[/b] - sound to play when the monster spawns.
[b]DamageSound[/b] - sound when the monster gets hit.
[b]DamageSound2[/b] - secondary sound on hit in combination with the first for a few monsters.
[b]ArmorSound[/b] - sound made when hitting the armor of some monster types (e.g. bugs)
[b]DeathSound[/b] - sound to play when the monster dies.
[b]DeathSound2[/b] - secondary sound on death in combination with the first for a few monsters.

[b]ProjectileSound[/b] - sound when firing a projectile.
[b]ProjectileSound2[/b] - secondary projectile sound for some monsters (e.g. on bounce).
[b]ProjectileIndex[/b] - the vanilla index for certain monster projectiles (see the game's TileSheets/Projectiles file).
[b]ProjectileDebuff[/b] - custom debuff for existing debuff projectiles.
[b]ProjectileSprite[/b] - a custom sprite path for a monster's projectile.
[b]ProjectileSource[/b] - a custom source rectangle for a projectile sprite.
[b]ProjectileScale[/b] - custom scale for certain projectiles (e.g. DinoMonster).
[b]ProjectileDamage[/b] - damage value for some projectiles.
[b]ProjectileRange[/b] - range of some projectiles.

AngryRoger specific

[b]SprinkleColor[/b] - color of sprinkles.


Bat specific

[b]CanLunge[/b] lunges
[b]CursedDoll[/b] is a cursed doll.
[b]HauntedSkull[/b] is a haunted skull (requires "CursedDoll": true to work).
[b]SpeedMin[/b] min value for max speed.
[b]SpeedMax[/b] max value for max speed.
[b]ExtraVelocity[/b] extra speed.


BigSlime specific

[b]HeldItem[/b] - the item to display inside the monster.


Bug specific

[b]Armored[/b] - whether the bug is armored.


Mummy specific
[b]CrumbleSound[/b] - sound when the monster crumbles.
[b]UncrumbleSound[/b] - sound when the monster gets back up.
[b]ReviveTimer[/b] - time in ms to revive. Vanilla is 10000.


RockCrab specific

[b]WaiterChance[/b] - chance to wait.
[b]ShellSound[/b] - sound of hitting the shell with a pickaxe.
[b]BreakSound[/b] - sound to make when the shell breaks.
[b]StickBug[/b] - true or false, is a stick bug.


Serpent specific

[b]MinSegment[/b] - minimum number of segments for a royal serpent.
[b]MaxSegment[/b] - maximum number of segments for a royal serpent.


Shooter specific

[b]ProjectileSpeed[/b] - speed of projectiles.
[b]ShotsPerFire[/b] - shots per fire.
[b]AimTime[/b] - time to aim when a player moves.
[b]BurstTime[/b] - time between shots.
[b]AimEndTime[/b] - time to aim after firing finishes.
[b]DesiredDistance[/b] - wants to be this far from players.
[b]FireRange[/b] - max firing range.

Spiker specific

[b]Vulnerable[/b] - whether monster can be damaged.


SquidKid specific

[b]ProjectileCount[/b] - how many projectiles to fire in one period.
[b]ProjectileTimer[/b] - how long in ms to wait between fire periods.






[b]ItemId[/b]
[b]MinQuantity[/b]
[b]MaxQuantity[/b]
[b]Quality[/b]
[b]Chance[/b] = 100;