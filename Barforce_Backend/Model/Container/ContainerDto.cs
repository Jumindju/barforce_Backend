namespace Barforce_Backend.Model.Container
{
    public class ContainerDto
    {
        public int Id { get; set; }
        public string MachineName { get; set; }
        public int FillingLevel { get; set; }
        public int FillingVolume { get; set; }
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public double AlcoholLevel { get; set; }
        public string Background { get; set; }
    }
}