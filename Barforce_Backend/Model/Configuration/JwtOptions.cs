namespace Barforce_Backend.Model.Configuration
{
    public class JwtOptions
    {
        public string Secret { get; set; }
        public int ExpireDays { get; set; }
    }
}