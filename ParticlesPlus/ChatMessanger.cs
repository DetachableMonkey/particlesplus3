using Vintagestory.API.Client;

namespace ParticlesPlus
{
    enum MessageType
    {
        Fail,
        Success        
    }
    internal class ChatMessanger
    {
        private readonly ModSystem _modSystem;
        private readonly ICoreClientAPI _capi;

        private readonly string _modName;
        private readonly string successColor = "#5FED6F";
        private readonly string failColor = "#ED5E60";


        public ChatMessanger(ICoreClientAPI capi, ModSystem modSystem)
        {
            _capi = capi;
            _modSystem = modSystem;
            _modName = _modSystem.Mod.Info.Name;
        }

        public void ShowMessage(string message, MessageType type)
        {
            string messageColor = type switch
            {
                MessageType.Success => successColor,
                MessageType.Fail => failColor,
                _ => "#FFFFFF",
            };

            _capi.ShowChatMessage($"<font color='{messageColor}'>[{_modName}]{message}</font>");
        }
    }
}
