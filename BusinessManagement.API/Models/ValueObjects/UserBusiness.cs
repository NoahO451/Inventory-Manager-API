namespace App.Models.ValueObjects
{
    public record UserBusiness
    {
        public UserBusiness(IReadOnlyCollection<Business>? businesses)
        {
            if (businesses != null && businesses.Count > 0 && businesses.Any(business => business.IsDeleted))
                throw new ArgumentException("Cannot include deleted businesses", nameof(businesses));

            Businesses = businesses;
        }

        public IReadOnlyCollection<Business>? Businesses { get; private set; }
    }
}
