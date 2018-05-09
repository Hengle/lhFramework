#if UNITY_EDITOR

namespace CodeStage.Maintainer
{
	public enum RecordLocation : byte
	{
		Unknown = 0,
		Scene = 5,
		Asset = 7,
		Prefab = 10,
		BuildSettings = 15,
		TagsAndLayers = 20,
	}
}

#endif