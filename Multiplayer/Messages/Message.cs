namespace ExitPath.Server.Multiplayer.Messages
{
    public record Message
    {
        public string Sender { get; init; }
        public string Text { get; init; }
        public int SenderColor { get; init; }
        public int TextColor { get; init; }

        private Message(string sender, string text, int senderColor, int textColor)
        {
            this.Sender = sender;
            this.Text = text;
            this.SenderColor = senderColor;
            this.TextColor = textColor;
        }

        public static Message System(string msg, int color = 0xffffff) =>
            new Message("@SYSTEM", msg, color, color);

        public static Message Error(string msg) => System(msg, 0xff0000);

        public static Message Player(Player player, string msg) =>
            new Message(player.Data.DisplayName, msg, player.Data.PrimaryColor, 0xffffff);
    }
}
