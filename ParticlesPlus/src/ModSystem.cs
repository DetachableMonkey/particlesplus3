using System;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using System.Linq;

namespace ParticlesPlus
{
    public class ModSystem : Vintagestory.API.Common.ModSystem
    {
        private static ModConfig modConfig;
        private ICoreClientAPI capi;
        private GuiDialog dialog;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Client;
        }
        public override void StartClientSide(ICoreClientAPI api)
        {
            capi = api;
            base.StartClientSide(capi);
            modConfig = new ModConfig(capi, this);
            dialog = new MainGuiDialog(capi, modConfig, this);   
            api.Input.RegisterHotKey(
                    "toggleParticles",
                    "Toggle Particles Plus",
                    GlKeys.P,
                    HotkeyType.HelpAndOverlays,         
                    shiftPressed: false,
                    ctrlPressed: true,
                    altPressed: false
                    );
            api.Input.SetHotKeyHandler("toggleParticles", OnHotkeyToggleParticles);
        }
        public override void AssetsFinalize(ICoreAPI api)
        {
            if (!modConfig.IsValid) return;
            if (modConfig != null && modConfig.Presets != null && modConfig.Particles != null)
            {
                ApplyConfigPresets(modConfig);
            }

            api.Logger.Event($"Started [{Mod.Info.Name}] mod");
        }
        private Block[] GetBlocks(string wildcard)
        {
            return capi.World.SearchBlocks(wildcard);
        }
        public void RemoveParticles(string wildcard)
        {
                Block[] blocks = GetBlocks(wildcard);
                foreach (Block block in blocks)
                {
                    block.ParticleProperties = Array.Empty<AdvancedParticleProperties>();
                }
        }
        public void AddParticles(string wildcard, AdvancedParticleProperties[] particles)
        {
                if (particles == null || particles.Length == 0) return;

                Block[] blocks = GetBlocks(wildcard);
                foreach (Block block in blocks)
                {
                    block.ParticleProperties ??= Array.Empty<AdvancedParticleProperties>();
                    block.ParticleProperties = block.ParticleProperties
                                    .Concat(particles)
                                    .ToArray();
                }
        }
        private bool OnHotkeyToggleParticles(KeyCombination keyComb)
        {
            GuiElementSwitch globalSwitch = dialog.Composers["single"].GetSwitch("globalSwitch");
            ToggleParticles(modConfig.Global);
            globalSwitch.SetValue(modConfig.Global);
            return true;
        }
        public void ToggleParticles(bool enabled)
        {
            if (enabled)
            {
                RemoveConfigPresets(modConfig);
                modConfig.Global = false;
                modConfig.WriteConfig();
            }
            else
            {
                modConfig.Global = true;
                ApplyConfigPresets(modConfig);
                modConfig.WriteConfig();
            }
        }
        private void ApplyConfigPresets(ModConfig config)
        {
            foreach (var preset in config.Presets)
            {
                if (!preset.Value.Enabled) continue; // Skip if disabled

                AdvancedParticleProperties[] particles = GetParticles(preset.Value.Particles);

                AddParticles(preset.Value.Wildcard, particles);
            }
        }
        private void RemoveConfigPresets(ModConfig config)
        {
            foreach (var preset in config.Presets)
            {
                if (!preset.Value.Enabled) continue; // Skip if disabled

                RemoveParticles(preset.Value.Wildcard);
            }
        }
        private static AdvancedParticleProperties[] GetParticles(string particlesKey)
        {
            return modConfig.Particles.TryGetValue(particlesKey, out var particles)
                ? particles
                : Array.Empty<AdvancedParticleProperties>();
        }
        public bool UpdateConfigPreset(string key, PresetConfig updatedPreset)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("[Particles Plus]: Preset name cannot be null or empty", nameof(key));

            if (!modConfig.Presets.TryGetValue(key, out PresetConfig targetPreset))
                return false;

            if (targetPreset == updatedPreset)
                return true;
           
            if (targetPreset.Enabled)
            {
                RemoveParticles(targetPreset.Wildcard);
            }

            if (updatedPreset.Enabled && modConfig.Global)
            {
                AdvancedParticleProperties[] particles = GetParticles(updatedPreset.Particles);
                AddParticles(updatedPreset.Wildcard, particles);
            }

            modConfig.Presets[key] = updatedPreset;
            modConfig.WriteConfig();

            return true;
        }
    }
}
