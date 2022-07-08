# Frontier Developments Shields

[![Version](https://img.shields.io/badge/Steam%20Workshop-1.2-green.svg)](https://steamcommunity.com/sharedfiles/filedetails/?id=1210535987)
[![License: CC BY-NC-SA 4.0](https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-blue.svg)](http://creativecommons.org/licenses/by-nc-sa/4.0/)
![Discord](https://img.shields.io/discord/629305389577666569)

Build shields. Stop bullets, rockets, explosions, drop pods, meteorites, super space lasers, and more!

## Features

#### Shields do:
- Stop projectiles (bullets, rockets, grenades, mortars, drop pods) from entering the shield
- Stop explosions from propagating into shields
- Stop skyfallers (drop pods, meteorites, ship debris)
- Stop bombardments (at *staggering* electricity cost!)
- Allows projectiles from inside to exit the shield
- Shields use a small amount of idle power
- Shield strength is the stored energy on your power net
- Shields generators will heat up as the shields absorb damage and will shut off if they overheat
- Shields exceeding their thermal threshold can suffer from three stages of breakdowns:
  - minor: Zzzt event with no fire or explosion
  - major: minor and a normal breakdown of the shield
  - critical: major, minor, and a small explosion

#### Shields Don't:
- Stop movement of pawns in or out of the shields
- Stop fires

#### AI

The AI has been modified to be aware of shields. Colonists will not attempt to hunt through shields. AI will not attempt
to shoot through shields that belong to their own faction. AI will prioritize shooting at unshielded targets and will 
prefer to take cover in areas that are shielded.

Tradeships will never shoot cargo pods into an area even if a shield is currently down. Raiders will not attempt to 
enter via drop pods in shielded areas.

Pawns will attempt to flee from orbital bombardments.

## Mods supported:
- Combat Extended: Integrated support handles CE automatically
- Crash Landing: Blocks crashing ship parts
- Centralized Climate Control: Cool shields with pipes
- Dubs Bad Hygiene: Cool shields with air-con pipes
- SRTS-Expanded ships can land and leave from shields
- AvoidFriendlyFire: Pawns will treat active shields providing cover

## Contributions

Contributions will be credited to the authors. Merged contributions will be owned by the mod itself (important for 
future license changes, but we anticipate that never happening).

### Extending

Please do! We designed this to be as modular as possible. New shields can be implemented from IShield like 
Comp_ShieldRadial. Handlers for what shields stop are best implemented as harmony patches like Harmony_Projectile.
