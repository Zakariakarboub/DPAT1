namespace FSMViewer.Model
{
    public class Trigger
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Trigger(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}