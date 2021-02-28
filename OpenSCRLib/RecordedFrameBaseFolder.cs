using LiteDB;

namespace OpenSCRLib
{
    public class RecordedFrameBaseFolder
    {
        [BsonCtor]
        public RecordedFrameBaseFolder()
        {
            Id = FixedId;

            BaseFolder = string.Empty;
        }

        [BsonCtor]
        public RecordedFrameBaseFolder(ObjectId _, string baseFolder)
        {
            Id = FixedId;

            BaseFolder = baseFolder;
        }

        public static ObjectId FixedId => new ObjectId("1");

        public ObjectId Id { get; set; }

        public string BaseFolder { get; set; }

        public override string ToString()
        {
            return $"RecordedFrameBaseFolder(Id={Id}, BaseFolder={BaseFolder})";
        }
    }
}
