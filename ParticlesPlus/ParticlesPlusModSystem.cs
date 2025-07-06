using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Newtonsoft.Json;

namespace ParticlesPlus
{
    public class ModConfig
    {
        public Dictionary<string, AdvancedParticleProperties[]> Data { get; set; }
    }
    public class ParticlesPlusModSystem : ModSystem
    {  
        public override void AssetsFinalize(ICoreAPI api)
        {
            string configName = $"{Mod.Info.ModID}.json";
            ModConfig config = null;

            try
            {
                config = api.LoadModConfig<ModConfig>(configName);
                if (config == null)
                {
                    string defaultConfig = api.Assets.Get(new AssetLocation(Mod.Info.ModID, "config/particlesplus.json")).ToText();
                    config = JsonConvert.DeserializeObject<ModConfig>(defaultConfig);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading config: {e.Message}");
            }
            
            if (config != null && config.Data != null)
            {
                foreach (Block block in api.World.Blocks)
                {
                    foreach (var wildCard in config.Data)
                    {
                        if (block.WildCardMatch(wildCard.Key))
                        {
                            block.ParticleProperties ??= Array.Empty<AdvancedParticleProperties>();
                            block.ParticleProperties = block.ParticleProperties.Append(wildCard.Value);
                        }
                    }
                }
            }
            api.World.Logger.Event($"Started {Mod.Info.Name} mod");
        }
    }
}
