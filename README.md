# Nexus

Unity survival prototype built around a day/night wave loop, third-person
character movement, weapon switching, melee attacks, inventory UI, and zombie
spawning.

## Unity Version

- Editor: Unity `6000.3.10f1`
- Render pipeline: Universal Render Pipeline
- Main scene: `Assets/Scenes/GameScene.unity`

## Project Layout

- `Assets/Nexus/FinalCharacterController` - player movement, camera, input, and
  animation helpers.
- `Assets/Nexus/Combat/Weapons` - weapon data, prefabs, icons, aiming, IK, and
  weapon manager logic.
- `Assets/Nexus/Systems/Core` - wave flow, day/night transitions, zombie AI,
  health, shop, and game manager systems.
- `Assets/Nexus/UserInventory` - inventory UI and slot/stat display scripts.
- `Assets/Scenes/GameScene` - baked scene-specific data such as the NavMesh
  manager asset.
- `Assets/Settings` - URP renderer assets, pipeline settings, and volume
  profiles.

## Gameplay Notes

- The game starts from `GameScene`.
- Daytime is used for preparation, buying upgrades, and opening the wave debug
  panel when enabled.
- Starting a wave transitions the world to night and spawns zombies based on the
  current wave number.
- Every tenth wave is treated as a stronger blood moon wave.
- The player can swap equipped slots, aim firearms, shoot, reload, and use melee
  strikes.

## Controls

Input is configured through Unity Input System assets:

- `Assets/InputSystem_Actions.inputactions`
- `Assets/New Actions.inputactions`

The combat map is consumed by `WeaponManager` and covers weapon slots, aim,
shoot, and reload actions. Movement input is handled by the
`FinalCharacterController` input scripts.

## Asset Attribution

This project includes a CC-BY model. Keep the attribution below in derivative
builds and public releases.

Baseball bat by jeremy [CC-BY](https://creativecommons.org/licenses/by/3.0/) via
Poly Pizza: https://poly.pizza/m/9FPflHIzK73
