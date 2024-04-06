namespace App.Models
{
    public class CustomPackaging
    {
        public long CustomPackageId { get; set; }
        public string Name { get; set; }
        public int WeightG { get; set; } // Weight in grams
        public int WidthCm { get; set; } // Package dimensions in centimeters
        public int HeightCm { get; set; }
        public int LengthCm { get; set; }
    }
}