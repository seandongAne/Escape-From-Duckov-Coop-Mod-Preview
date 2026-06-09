// Escape-From-Duckov-Coop-Mod-Preview
// Copyright (C) 2025  Mr.sans and InitLoader's team
//
// This program is not a free software.
// It's distributed under a license based on AGPL-3.0,
// with strict additional restrictions:
//  YOU MUST NOT use this software for commercial purposes.
//  YOU MUST NOT use this software to run a headless game server.
//  YOU MUST include a conspicuous notice of attribution to
//  Mr-sans-and-InitLoader-s-team/Escape-From-Duckov-Coop-Mod-Preview as the original author.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.

namespace EscapeFromDuckovCoopMod;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    private static bool _harmonyPatched;
    public Harmony Harmony;

    public void OnEnable()
    {
        if (!_harmonyPatched)
        {
            Harmony = new Harmony("DETF_COOP");
            Harmony.PatchAll();
            _harmonyPatched = true;
        }
        else
        {
            Debug.Log("[NET_STATE] ModBehaviour.OnEnable ignored duplicate Harmony patch request");
        }

        CoopLocalization.Initialize();

        var go = GetOrCreateRoot("COOP_MOD_1");
        EnsureComponent<NetService>(go);
        COOPManager.InitManager();
        EnsureComponent<ModBehaviourF>(go);
        Loader();
    }

    public void Loader()
    {
        var go = GetOrCreateRoot("COOP_MOD_");

        EnsureComponent<SteamP2PLoader>(go);
        EnsureComponent<Send_ClientStatus>(go);
        EnsureComponent<HealthM>(go);
        EnsureComponent<LocalPlayerManager>(go);
        EnsureComponent<SendLocalPlayerStatus>(go);
        EnsureComponent<SendLocalVehicleStatus>(go);
        EnsureComponent<Spectator>(go);
        EnsureComponent<DeadLootBox>(go);
        EnsureComponent<LootManager>(go);
        EnsureComponent<SceneNet>(go);
        EnsureComponent<MModUI>(go);
        EnsureComponent<CoopAISettings>(go);
        EnsureComponent<AISyncSettingsUI>(go);
        EnsureComponent<CoopLootSettings>(go);
        EnsureComponent<WaitingSynchronizationUI>(go);
        EnsureComponent<VersionOverlayTMP>(go);
        CoopTool.Init();

        DeferredInit();
    }

    private void DeferredInit()
    {
        SafeInit<SteamP2PLoader>(s => s.Init());
        SafeInit<SceneNet>(sn => sn.Init());
        SafeInit<LootManager>(lm => lm.Init());
        SafeInit<LocalPlayerManager>(lpm => lpm.Init());
        SafeInit<HealthM>(hm => hm.Init());
        SafeInit<SendLocalPlayerStatus>(s => s.Init());
        SafeInit<SendLocalVehicleStatus>(s => s.Init());
        SafeInit<Spectator>(s => s.Init());
        SafeInit<MModUI>(ui => ui.Init());
        SafeInit<CoopLootSettings>(s => s.Init());
        SafeInit<CoopAISettings>(s => s.Init());
        SafeInit<AISyncSettingsUI>(ui => ui.Init());
        SafeInit<Send_ClientStatus>(s => s.Init());
        SafeInit<DeadLootBox>(s => s.Init());
        
    }

    private void SafeInit<T>(Action<T> init) where T : Component
    {
        var c = FindObjectOfType<T>();
        if (c == null) return;
        try
        {
            init(c);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NET_STATE] Init failed for {typeof(T).Name}: {ex}");
        }
    }

    private static GameObject GetOrCreateRoot(string name)
    {
        var go = GameObject.Find(name);
        if (go != null)
            return go;

        go = new GameObject(name);
        DontDestroyOnLoad(go);
        Debug.Log($"[NET_STATE] created persistent root '{name}'");
        return go;
    }

    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        // 热重载或重复启用时只补缺失组件，避免把全局服务堆成多份。
        var component = go.GetComponent<T>();
        if (component != null)
            return component;

        return go.AddComponent<T>();
    }
}
