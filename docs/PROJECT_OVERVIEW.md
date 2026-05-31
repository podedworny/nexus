# Project Overview

## Scenes

- `Assets/Scenes/GameScene.unity` is the only scene currently enabled in build
  settings.
- `Assets/Scenes/GameScene/NavMesh-NavMeshManager.asset` stores scene-specific
  navigation data used by zombie movement.
- Vendor sample scenes are kept under their package folders and are not part of
  the active build scene list.

## Core Prefabs And Data

- Weapons are assembled from `Assets/Nexus/Combat/Weapons/Prefabs`.
- Weapon inventory entries are stored as ScriptableObjects in
  `Assets/Nexus/Combat/Weapons/Assets`.
- Weapon and item behavior is driven by data classes in
  `Assets/Nexus/Combat/Weapons/Data`.
- The zombie prefab source lives in the imported animation/model packs and is
  configured by scene references through `EnemySpawner`.

## Runtime Flow

1. `GameManager` keeps checkpoint state, player lives, currency, and upgrade
   multipliers.
2. `WaveManager` controls day/night state, starts night waves, tracks living
   zombies, and saves checkpoints at the end of each wave.
3. `EnemySpawner` creates normal, mini-boss, and boss zombies from the current
   wave number.
4. `ZombieAI` uses NavMesh movement to chase the player and triggers attack
   animations inside melee range.
5. `WeaponManager` handles slot selection, aim, shooting, reloads, melee
   attacks, ammo state, recoil, and HUD ammo updates.

## Project Settings Check

- Version control mode is set to `Visible Meta Files`.
- Asset serialization mode is `Force Text`.
- Active render setup uses URP assets under `Assets/Settings`.
- Packages are locked through `Packages/packages-lock.json`.
