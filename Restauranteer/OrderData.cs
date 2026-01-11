namespace Restauranteer
{
    internal class OrderData
    {
        public int dishIndex;
        public string dishId;
        public string dishName;
        public string dishDisplayName;
        public int dishPrice;
        public bool loved;

        public OrderData(int index, string dishId, string dishName, string dishDisplayName, int dishPrice, bool loved)
        {
            this.dishIndex = index;
            this.dishId = dishId;
            this.dishName = dishName;
            this.dishDisplayName = dishDisplayName;
            this.dishPrice = dishPrice;
            this.loved = loved;
        }
    }
}