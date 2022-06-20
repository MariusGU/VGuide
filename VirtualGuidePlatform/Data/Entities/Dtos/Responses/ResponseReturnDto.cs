namespace VirtualGuidePlatform.Data.Entities.Dtos
{
    public class ResponseReturnDto
    {
        public string? _id { get; set; }
        public string Text { get; set; }
        public double Rating { get; set; }
        public string GId { get; set; }
        public string UId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Ppicture { get; set; }

        public ResponseReturnDto(Responses response, string firstName, string lastName, string ppicture)
        {
            _id = response._id;
            Text = response.text;
            Rating = response.rating;
            GId = response.gId;
            UId = response.uId;
            FirstName = firstName;
            LastName = lastName;
            Ppicture = ppicture;
        }
    }
}
