namespace PoETradeAlert
{
    class Whisper
    {
        public string Price { get; set; }
        public string Item { get; set; }
        public string Stash { get; set; }

        public string Format()
        {
            return $"<strong>{Price}</strong> - <u>{Item}</u> - <i>{Stash}</i>";
        }
    }
}
