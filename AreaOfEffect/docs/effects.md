# Spell Effects

Spell effects are applied to a variety of objects including the tiles themselves and any entity on a tile in the AOE.

Different effect types will require different data. Generic effect fields are:

| Field | Type | Description |
|:-----:|:----:|:------------|
| Affected | string[] | Types of objects affected by the effect. |
| Unaffected | string[] | Types of objects unaffected by the effect (used for sub-types, e.g. Trees vs TerrainFeatures). |
| EffectType | string | The type of effect to apply (see below). |
| Value | varies | A generic variable used by various effect types for different purposes. |


## Affected Types

You can specify multiple affected types for each effect, and optionally specify unaffected types to exclude sub-types. For example, you could cause the effect to affect all TerrainFeatures except Trees by specifying "TerrainFeature" in Affected and "Tree" in Unaffected. 

Here is a list of implemented affected types:

| Type | Description |
|:----:|:------------|
| Animal | farm animals. |
| Building |  |
| Crop |  |
| Farmer | player characters (yes, friendly-fire is possible) |
| FruitTree |  |
| Grass |  |
| HoeDirt | tilled soil |
| Horse |  |
| Monster |  |
| NPC | human NPCs |
| Object | placed objects like furnaces, etc. |
| Pet |  |
| ResourceClump | large four-tile objects like stumps and boulders |
| Stone | breakable rocks  |
| Tile | the tile itself |
| Tree |  |
| Twig |  |
| Weed |  |

Different objects are affected differently by different effect types.


## Effect Types

Here is a list of implemented spell effects:

| Effect | Can Affect | Description |
|:------:|:-------:|:------------|
| Buff | Farmer, Monster | Applies a buff to the target (monster buffing is limited atm to those that effect their speed or cause them to freeze). |
| Burn | Tile, TerrainFeature, Crop, Object, ResourceClump | Causes the target to catch fire, being destroyed after a time. |
| Custom | all except Tile | Change a custom C# field or property for the target. |
| Damage | Monster, Farmer | Reduce health as though hit. |
| Destroy | ResourceClump, Crop, Object, TerrainFeature | Remove the target completely. |
| Explode | Tile | Cause an explosion at the given tile. |
| Fertilize | HoeDirt | Apply fertilizer. |
| Freeze | Monster, Farmer | Cause the target to halt for a duration. |
| Grow | Grass, Tree, FruitTree, Crop | Causes the target to grow fully. |
| Heal | Farmer | Increases health. |
| Harvest | Crop, Animal, Grass, FruitTree | Harvest as though done by the farmer. |
| Invincible | Farmer | Make a player temporarily invincible. |
| Index |  Tile | Not yet implemented. |
| Light | Farmer, Tile, Monster | Applies a temporary lightsource attached to the target. |
| ModData | all except Tile | Sets a key value pair in the target's mod data dictionary. |
| Pet | Pet, Animal | Pets the target.  |
| Plant | HoeDirt | Plant a crop at the target. |
| Property | Tile | Not yet implemented. |
| TileSheet | Tile | Not yet implemented. |
| Tool | Tile, TerrainFeature, ResourceClump, Object, Crop | Perform a tool action on the target. |
| Transform |  | Not yet implemented. |
| Water | Tile, HoeDirt, Building | Water the target. |