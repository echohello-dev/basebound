# Basebound FAQ

## Networking & Spawn

**Q: Why is `PlayerClient.Local` or `PlayerClient.Viewer` null?**  
A: No `PlayerClient` was spawned/owned. Ensure `PlayerClientSpawner` exists in `game_setup.prefab` and you’re running as host/listen server.

**Q: Why does the pawn spawn but HUD shows no data?**  
A: The pawn must be bound to a `PlayerClient`. `PlayerPawnBinder` handles this on `OnNetworkSpawn` and calls `PlayerClient.OnPossess`.

**Q: Is a pawn binder expected in other gamemodes?**  
A: In hc1-style setups (separate Client + Pawn), yes — a possess/bind step is expected. In walker/jumpgame-style setups, the player is a single prefab, so no binder is needed.

**Q: Who spawns the pawn?**  
A: `Sandbox.NetworkHelper` spawns `player_pawn.prefab` on `OnActive` and assigns ownership.

**Q: Who spawns the client object?**  
A: `PlayerClientSpawner` spawns `player_client.prefab` on `OnActive` (host only).

**Q: Why can’t I run debug commands as a client?**  
A: Health/ammo changes are host authoritative. Run commands on the host/listen server.

**Q: Where does the pawn spawn?**  
A: `NetworkHelper` uses `SpawnPoints` if set, otherwise `SpawnPoint` components in the scene, or its own transform.

## HUD & Ammo

**Q: Why does ammo HUD show 0?**  
A: The pawn must have `WeaponComponent` and be possessed. Use `bb_add_weapon` after spawn.

**Q: How do I test ammo without firing?**  
A: Use `bb_set_ammo`, `bb_consume_ammo`, `bb_reload` from the console (host only).

## Debug Commands

**Q: How do I inspect current client/pawn state?**  
A: Run `bb_dump_clients` to log Local/Viewer and per-client pawn status.

## Scene Setup

**Q: What should `minimal.scene` contain?**  
A: `game_setup.prefab` (with `NetworkHelper` + `PlayerClientSpawner`) and no static `player_client` object.

**Q: What should `player_pawn.prefab` include?**  
A: `PlayerController`, movement components, `WeaponComponent`, and `PlayerPawnBinder`.
