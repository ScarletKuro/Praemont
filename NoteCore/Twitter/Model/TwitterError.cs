using System.Runtime.Serialization;

namespace NoteCore.Twitter.Model
{
    [DataContract]
    public class TwitterError
    {
        [DataMember(Name = "code")]
        public int Code { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }
    }

    [DataContract]
    public class TwitterErrors
    {
        [DataMember(Name = "errors")]
        public TwitterError[] Errors { get; set; }
    }
}