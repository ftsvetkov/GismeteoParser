using System;

namespace GismeteoParser
{
    public class City
    {
        /// <summary>
        /// Id
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Path
        /// </summary>
        public String Path { get; set; }

        public City(string name, string path, int? id)
        {
            Name = name;
            Path = path;
            Id = id;
        }

        public City()
        { }
    }
}
