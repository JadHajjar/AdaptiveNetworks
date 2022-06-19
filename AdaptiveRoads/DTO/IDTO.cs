using System.IO;
using System.Xml.Serialization;

namespace AdaptiveRoads.DTO {
    public interface IDTO<T> where T: class, new(){
        void ReadFromGame(T gameData);
        void WriteToGame(T gameData);
    }

    public interface ISerialziableDTO {
        void Save();
        void OnLoaded(FileInfo file);
        string Name { get; set; }
        string Description { get; set; }
        string Summary { get; }
        [XmlAttribute]string Version { get; set; }
    }
}