/// <summary>
/// Chapter 1 key ring catalog (13 keys) — data source for S6 grid.
/// </summary>
public static class KeyCatalog
{
    public static readonly string[] AllKeyIds = ArtPaths.AllKeyIds;
    public static int Count => AllKeyIds.Length;

    public static string DisplayName(string keyId) => ArtPaths.KeyDisplayName(keyId);
    public static string SpritePath(string keyId) => ArtPaths.KeySpriteForId(keyId);
}