namespace Barforce_Backend.Model.Container
{
    public class ContainerDto : Ingredient.Ingredient
    {
        public int Id { get; set; }
        public string MachineName { get; set; }
        public int FillingLevel { get; set; }
        public int FillingVolume { get; set; }
    }
}