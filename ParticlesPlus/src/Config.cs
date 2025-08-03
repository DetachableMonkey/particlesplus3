using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ParticlesPlus
{

    public record PresetConfig
    {
        public bool Enabled { get; set; }
        public string Wildcard { get; set; } = "";
        public string Particles { get; set; } = "";
    }
    public class ModConfig
    {
        public int Version { get; set; }
        public bool Global { get; set; } = true;
        public Dictionary<string, PresetConfig> Presets { get; set; } = new();
        public Dictionary<string, AdvancedParticleProperties[]> Particles { get; set; } = new();

        [JsonIgnore]
        private readonly ICoreClientAPI capi;
        [JsonIgnore]
        public bool IsValid = true;
        [JsonIgnore]
        private readonly string configFileName;

        public ModConfig() { }
        public ModConfig(ICoreClientAPI capi, ModSystem modSystem)
        {
            this.capi = capi;

            configFileName = $"{modSystem.Mod.Info.ModID}.json";
            try
            {
                var loadedConfig = capi.LoadModConfig<ModConfig>(configFileName);

                if (loadedConfig == null)
                {
                    var defaultConfigAsset = capi.Assets.Get(new AssetLocation(modSystem.Mod.Info.ModID, "config/particlesplus.json"));
                    string defaultConfigText = defaultConfigAsset.ToText();

                    loadedConfig = JsonConvert.DeserializeObject<ModConfig>(defaultConfigText);

                    CopyFrom(loadedConfig);
                    WriteConfig();
                }
                else
                {
                    if (loadedConfig.Version == 0)
                    {
                        capi.Logger.Error($"[{modSystem.Mod.Info.Name}] Config file is missing required 'Version' field (old or malformed config). Please regenerate or update it.");
                        IsValid = false;
                        return;
                    }
                    CopyFrom(loadedConfig);
                }
            }
            catch (Exception e)
            {
                capi.Logger.Error($"[{modSystem.Mod.Info.Name}] Failed to load config file: {e.Message}");
                IsValid = false;
                return;
            }
        }
        private void CopyFrom(ModConfig modConfig)
        {
            if (modConfig == null) return;

            Version = modConfig.Version;
            Global = modConfig.Global;
            Presets = modConfig.Presets ?? new();
            Particles = modConfig.Particles ?? new();
        }
        public void WriteConfig()
        {
            capi.StoreModConfig(this, configFileName);
        }
    }
}
