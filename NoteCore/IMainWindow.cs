using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteCore.Model;

namespace NoteCore
{
    public interface IMainWindow
    {
        bool LoadedForm { get; set; }
        void ShowUserInfo(string name);

        void OnReply(Tweet tweet, string screenName);
    }
}
