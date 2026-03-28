namespace Common
{
    public class Leaders
    {
        public int Id { get; set; }
        public string Name { get; set; }  // ← должно быть string, а не int
        public int Points { get; set; }
    }
}