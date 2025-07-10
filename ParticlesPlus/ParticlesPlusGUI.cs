using HarmonyLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using System;

namespace ParticlesPlus
{
    public class MainGuiDialog : GuiDialog
    {
        public override string ToggleKeyCombinationCode => "particlesplus";
        private readonly ModConfig modConfig;
  
        public MainGuiDialog(ICoreClientAPI capi, ModConfig config) : base(capi)
        {
            modConfig = config;
            SetupDialog();
            capi.Gui.RegisterDialog(this);
            capi.Input.RegisterHotKey(ToggleKeyCombinationCode, "Particles Plus GUI", GlKeys.U, HotkeyType.GUIOrOtherControls);

        }
        private void SetupDialog()
        {
            int elementWidth = 300;
            int labelHeight = 16;
            int dropdownHeight = 30;
            int spacing = 10;
            int centeredLabelY = dropdownHeight / 2 - labelHeight / 2;


            // Auto-sized dialog at the center of the screen
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            // Preset Dropdown
            ElementBounds presetDropdownLabelBounds = ElementBounds.Fixed(0, GuiStyle.TitleBarHeight, elementWidth, labelHeight);
            ElementBounds presetDropdownBounds = ElementBounds.Fixed(0,0,elementWidth,dropdownHeight).FixedUnder(presetDropdownLabelBounds, spacing);

            //// Particles Dropdown
            ElementBounds particlesDropdownLabelBounds = ElementBounds.Fixed(0, 0, elementWidth, labelHeight).FixedUnder(presetDropdownBounds, spacing * 2);
            ElementBounds particlesDropdownBounds = ElementBounds.Fixed(0,0,elementWidth,dropdownHeight).FixedUnder(particlesDropdownLabelBounds, spacing);

            //// Switch
            ElementBounds switchBounds = ElementBounds.Fixed(0, 0, dropdownHeight, dropdownHeight).FixedUnder(particlesDropdownBounds, spacing * 2);
            ElementBounds switchLabelBounds = ElementBounds.Fixed(0, centeredLabelY, elementWidth - dropdownHeight, labelHeight)
                .FixedRightOf(switchBounds, spacing)
                .FixedUnder(particlesDropdownBounds, spacing * 2);

            // Background boundaries. Again, just make it fit it's child elements, then add the text as a child element
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren
                (
                presetDropdownLabelBounds, 
                presetDropdownBounds, 
                particlesDropdownLabelBounds, 
                particlesDropdownBounds, 
                switchBounds, 
                switchLabelBounds
                );

            string[] presetValues = modConfig.Presets.Keys.ToArray();
            string[] presetNames = modConfig.Presets.Keys.ToArray();
            string[] particlesValues = modConfig.Particles.Keys.ToArray();
            string[] particlesNames = modConfig.Particles.Keys.ToArray();

            // Lastly, create the dialog
            SingleComposer = capi.Gui.CreateCompo("myAwesomeDialog", dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Particles Plus Configuration", OnTitleBarCloseClicked)
                .AddStaticText("Preset:",CairoFont.WhiteSmallText().WithLineHeightMultiplier(1.5f), presetDropdownLabelBounds)
                .AddDropDown(presetValues, presetNames, 0, HandlePresetSelection, presetDropdownBounds, "presetDropdown")
                .AddStaticText("Particles:", CairoFont.WhiteSmallText(), particlesDropdownLabelBounds)
                .AddDropDown(particlesValues, particlesNames, 0, HandleParticlesSelection, particlesDropdownBounds, "particlesDropdown")
                .AddSwitch(HandleEnabledSwitch, switchBounds, size: dropdownHeight, key: "enabledSwitch")
                .AddStaticText("Enabled", CairoFont.WhiteSmallText(), switchLabelBounds)
                .Compose();

            HandlePresetSelection(presetValues[0], true);
        }
        private void HandlePresetSelection(string code, bool selected)
        {
            string currentParticles = modConfig.Presets[code].Particles;
            bool enabled = modConfig.Presets[code].Enabled;

            SingleComposer.GetDropDown("particlesDropdown").SetSelectedValue(currentParticles);
            SingleComposer.GetSwitch("enabledSwitch").SetValue(enabled);
        }
        private void HandleParticlesSelection(string code, bool selected)
        {
            string selectedPresetKey = SingleComposer.GetDropDown("presetDropdown").SelectedValue;
            string wildCard = modConfig.Presets[selectedPresetKey].Mask;
            modConfig.Presets[selectedPresetKey].Particles = code;

            AdvancedParticleProperties[] particles = modConfig.Particles[code];
            Block[] blocks = capi.World.SearchBlocks(wildCard);

            RemoveParticles(blocks);
            AddParticles(blocks, particles);
        }
        private void HandleEnabledSwitch(bool enabled)
        {
            string selectedPresetKey = SingleComposer.GetDropDown("presetDropdown").SelectedValue;
            string particlesName = modConfig.Presets[selectedPresetKey].Particles;
            string wildCard = modConfig.Presets[selectedPresetKey].Mask;
            AdvancedParticleProperties[] particles = modConfig.Particles[particlesName];

            modConfig.Presets[selectedPresetKey].Enabled = enabled;

            Block[] blocks = capi.World.SearchBlocks(wildCard);

            if (!enabled)
            {
                RemoveParticles(blocks);
            }
            else if (enabled) 
            {
                AddParticles(blocks, particles);
            }
                
        }
        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }
        private void RemoveParticles(Block[] blocks)
        {
            foreach (Block block in blocks)
            {
                block.ParticleProperties = Array.Empty<AdvancedParticleProperties>();
            }
        }
        private void AddParticles(Block[] blocks, AdvancedParticleProperties[] particles)
        {
            foreach (Block block in blocks)
            {
                block.ParticleProperties ??= Array.Empty<AdvancedParticleProperties>();
                block.ParticleProperties = block.ParticleProperties
                                .Concat(particles)
                                .ToArray();
            }
        }
    } 
}