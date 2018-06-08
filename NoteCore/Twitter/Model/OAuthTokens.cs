using ProtoBuf;

namespace NoteCore.Twitter.Model
{
    [ProtoContract(SkipConstructor = true)]
    public class OAuthTokens
    {
        [ProtoMember(1)]
        public string OAuthToken { get; set; }

        [ProtoMember(2)]
        public string OAuthSecret { get; set; }

        [ProtoMember(3)]
        public string UserId { get; set; }

        [ProtoMember(4)]
        public string ScreenName { get; set; }

        public OAuthTokens()
        {
            OAuthToken = string.Empty;
            OAuthSecret = string.Empty;
            UserId = string.Empty;
            ScreenName = string.Empty;
        }
    }
}
