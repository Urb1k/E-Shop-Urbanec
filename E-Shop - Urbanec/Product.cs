namespace E_Shop___Urbanec
{
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public byte[] Image { get; set; }
        public decimal Price { get; set; }
        public bool HasCustomImage { get; set; }
    }
}
