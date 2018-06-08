using ProtoBuf;

namespace NoteCore.Utils
{
	public static class ClassSerialization
	{
		public static void Save<T>(string filename, T obj) where T : class
		{
            using (var stream = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Write))
            {
                Serializer.Serialize(stream, obj);
            }

		}
		public static T Load<T>(string filename) where T : class
		{
		    using (var stream = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
			{
			    return Serializer.Deserialize<T>(stream);
			}
		}
	}
}
